using System.Data.Entity.Infrastructure;

namespace mvcDapper3.AppCodes.AppRepo;

public class AuthRepo : IAuthRepo
{
    private readonly IConnFactory _connFactory;
    private readonly ICrypto _crypto;

    public AuthRepo(IConnFactory connFactory, ICrypto crypto)
    {
        _connFactory = connFactory;
        _crypto = crypto;
    }

    public async Task<Users> AuthUserAsync(string userno, string password)
    {
        try
        {
            using (var conn = _connFactory.GetOpenConn())
            {
                const string query = @"SELECT 
                UserNo, UserName, Password, RoleNo, CodeNo, DeptNo, ContactEmail, 
                Settings, CalendarPreference
                FROM dbo.Users 
                WHERE UserNo = @UserNo AND IsValid = 1";

                var user = await conn.QueryFirstOrDefaultAsync<Users>(query, new { UserNo = userno });

                if (user != null && _crypto.VerifyPassword(password, user.Password))
                {
                    return user;
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error authenticating user");
            return null;
        }
    }
    public async Task<bool> ChkUsernameExistAsync(string username)
    {
        try
        {
            using (var conne = _connFactory.GetOpenConn())
            {
                var query = @"SELECT COUNT(*) 
                FROM dbo.Users 
                WHERE UserNo = @UserNo";
                var count = await conne.ExecuteScalarAsync<int>(query, new { UserNo = username });
                return count > 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking if username exists");
            throw;
        }
    }

    public async Task RegisterUserAsync(Users user, string passwordHash)
    {
        try
        {
            using (var conn = _connFactory.GetOpenConn())
            {
                var query = @"INSERT INTO dbo.Users 
                (UserNo, UserName, RoleNo, ContactEmail, Password, IsValid, ActivationToken, TokenExpiry, Settings)
                VALUES (@UserNo, @UserName, @RoleNo, @ContactEmail, @PasswordHash, 0, @ActivationToken, @TokenExpiry, @Settings)";

                await conn.ExecuteAsync(query, new
                {
                    user.UserNo,
                    user.UserName,
                    user.RoleNo,
                    user.ContactEmail,
                    PasswordHash = passwordHash,
                    user.ActivationToken,
                    user.TokenExpiry,
                    Settings = (object)user.Settings ?? DBNull.Value
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering user");
            throw;
        }
    }
    public async Task<bool> ActivateUserAsync(string token)
    {
        try
        {
            using (var conn = _connFactory.GetOpenConn())
            {
                // Check if token exists and is not expired
                var query = @"SELECT UserNo 
                FROM dbo.Users
                WHERE ActivationToken = @Token
                AND TokenExpiry > GETUTCDATE()
                AND IsValid = 0";

                var username = await conn.QueryFirstOrDefaultAsync<string>(query, new { Token = token });

                if (string.IsNullOrEmpty(username))
                {
                    return false;
                }

                // Update user to activated
                query = @"UPDATE dbo.Users
                SET IsValid = 1,
                    ActivationToken = NULL,
                    TokenExpiry = NULL
                WHERE ActivationToken = @Token";

                var rowsAffected = await conn.ExecuteAsync(query, new { Token = token });

                // Verify update was successful
                query = @"SELECT IsValid 
                FROM dbo.Users 
                WHERE UserNo = @UserNo";
                var isValid = await conn.QueryFirstOrDefaultAsync<bool>(query,
                new { UserNo = username });

                return isValid;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error activating user account");
            return false;
        }
    }
    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        try
        {
            using (var conn = _connFactory.GetOpenConn())
            {
                // Check if user exists
                var query = @"SELECT UserNo 
                FROM dbo.Users 
                WHERE ContactEmail = @Email 
                AND IsValid = 1";

                var userNo = await conn.QueryFirstOrDefaultAsync<string>(query,
                new { Email = email });

                if (string.IsNullOrEmpty(userNo))
                {
                    return null;
                }

                // Generate token
                var token = Guid.NewGuid().ToString();
                var expiryTime = DateTime.UtcNow.AddMinutes(10); // Token expires in 10 minutes

                // Update user with token
                query = @"UPDATE dbo.Users 
                SET ActivationToken = @Token, 
                    TokenExpiry = @Expiry 
                WHERE UserNo = @UserNo";

                await conn.ExecuteAsync(query, new
                {
                    Token = token,
                    Expiry = expiryTime,
                    UserNo = userNo
                });

                return token;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating password reset token");
            return null;
        }
    }
    public async Task<bool> ResetPasswordTokenAsync(string token, string newPasswordHash)
    {
        try
        {
            using (var conn = _connFactory.GetOpenConn())
            {
                // Check if token exists and is not expired
                var query = @"SELECT UserNo 
                FROM dbo.Users
                WHERE ActivationToken = @Token
                AND TokenExpiry > GETUTCDATE()
                AND IsValid = 1";

                var userNo = await conn.QueryFirstOrDefaultAsync<string>(query,
                new { Token = token });

                if (string.IsNullOrEmpty(userNo))
                {
                    return false;
                }

                // Update password and clear token
                query = @"UPDATE dbo.Users
                SET Password = @PasswordHash,
                    ActivationToken = NULL,
                    TokenExpiry = NULL
                WHERE UserNo = @UserNo";

                var rowsAffected = await conn.ExecuteAsync(query, new
                {
                    PasswordHash = newPasswordHash,
                    UserNo = userNo
                });

                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting password with token");
            return false;
        }
    }

    public async Task<Users> GetSocialLoginUserAsync(string provider, string providerKey)
    {
        try
        {
            using (var conn = _connFactory.GetOpenConn())
            {
                const string query = @"SELECT 
                    Id, IsValid, UserNo, UserName, Password, CodeNo, RoleNo, GenderCode, 
                    DeptNo, TitleNo, Birthday, OnboardDate, LeaveDate, ContactEmail, 
                    ContactTel, ContactAddress, ValidateCode, NotifyPassword, Remark, 
                    ActivationToken, TokenExpiry, Settings, CalendarPreference
                    FROM dbo.Users 
                    WHERE CodeNo = @Provider AND ValidateCode = @ProviderKey AND IsValid = 1";

                var user = await conn.QueryFirstOrDefaultAsync<Users>(query, new 
                { 
                    Provider = provider, 
                    ProviderKey = providerKey 
                });

                return user;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting social login user for provider {Provider} and key {ProviderKey}", provider, providerKey);
            return null;
        }
    }

    public async Task<Users> CreateSocialLoginUserAsync(string provider, string providerKey, string providerDisplayName, string email)
    {
        try
        {
            // First check if user already exists
            var existingUser = await GetSocialLoginUserAsync(provider, providerKey);
            if (existingUser != null)
            {
                Log.Information("Social login user already exists for provider {Provider} and key {ProviderKey}", provider, providerKey);
                return existingUser;
            }

            using (var conn = _connFactory.GetOpenConn())
            {
                var userNo = Guid.NewGuid().ToString().Replace("-", "");
                
                const string insertQuery = @"
                    INSERT INTO dbo.Users
                    (IsValid, CodeNo, UserNo, UserName, Password, RoleNo, GenderCode, DeptNo, TitleNo,
                    ContactTel, ContactEmail, ContactAddress, ValidateCode, Remark)
                    VALUES
                    (@IsValid, @CodeNo, @UserNo, @UserName, @Password, @RoleNo, @GenderCode, @DeptNo, @TitleNo,
                    @ContactTel, @ContactEmail, @ContactAddress, @ValidateCode, @Remark)";

                await conn.ExecuteAsync(insertQuery, new
                {
                    IsValid = true,
                    CodeNo = provider,
                    UserNo = userNo,
                    UserName = providerDisplayName,
                    Password = "", // Empty password for social login
                    RoleNo = "Member",
                    GenderCode = "M", // Default gender
                    DeptNo = "",
                    TitleNo = "",
                    ContactTel = "",
                    ContactEmail = email,
                    ContactAddress = "",
                    ValidateCode = providerKey,
                    Remark = $"Social login user created via {provider}"
                });

                Log.Information("Created new social login user for provider {Provider}, email {Email}", provider, email);

                // Return the newly created user
                return await GetSocialLoginUserAsync(provider, providerKey);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating social login user for provider {Provider} and email {Email}", provider, email);
            throw;
        }
    }
}
