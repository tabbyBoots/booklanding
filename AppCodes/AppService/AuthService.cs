using mvcDapper3.Models.ViewModel;
namespace mvcDapper3.AppCodes.AppService;

public class AuthService : IAuthService
{
    private readonly IAuthRepo _authRepo;
    private readonly ICrypto _crypto;

    public AuthService(IAuthRepo authRepo, ICrypto crypto)
    {
        _authRepo = authRepo;
        _crypto = crypto;
    }

    public async Task<Users> AuthUserAsync(string userno, string password)
    {
        return await _authRepo.AuthUserAsync(userno, password);
    }

    public async Task<bool> ChkUsernameExistAsync(string username)
    {
        return await _authRepo.ChkUsernameExistAsync(username);
    }
    public async Task<string> RegisterUserAsync(vmRegister model)
    {
        try
        {
            // Create activation token
            var activationToken = Guid.NewGuid().ToString();

            // Create new user
            var newUser = new Users
            {
                UserNo = model.Username,
                UserName = model.Username,
                RoleNo = model.RoleNo,
                ContactEmail = model.Email,
                IsValid = false,
                ActivationToken = activationToken,
                TokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // Hash password
            var passwordHash = _crypto.CreatePasswordHash(model.Password);

            // Register user
            await _authRepo.RegisterUserAsync(newUser, passwordHash);
            
            return activationToken;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering user");
            throw;
        }
    }
    public async Task<bool> ActivateUserAsync(string token)
    {
        return await _authRepo.ActivateUserAsync(token);
    }
    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        return await _authRepo.GeneratePasswordResetTokenAsync(email);
    }

    public async Task<bool> ResetPasswordTokenAsync(vmResetPassword model, string token)
    {
        try
        {
            // Hash new password
            var passwordHash = _crypto.CreatePasswordHash(model.Password);

            // Reset password
            return await _authRepo.ResetPasswordTokenAsync(token, passwordHash);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting password");
            throw new PasswordResetException("An error occurred while resetting your password.", ex);
        }
    }

    public async Task<Users> AuthenticateSocialUserAsync(string provider, string providerKey, string providerDisplayName, string email)
    {
        try
        {
            // First try to get existing social login user
            var existingUser = await _authRepo.GetSocialLoginUserAsync(provider, providerKey);
            
            if (existingUser != null)
            {
                Log.Information("Existing social login user found for provider {Provider}, email {Email}", provider, email);
                return existingUser;
            }

            // Create new social login user if not exists
            var newUser = await _authRepo.CreateSocialLoginUserAsync(provider, providerKey, providerDisplayName, email);
            
            if (newUser != null)
            {
                Log.Information("New social login user created for provider {Provider}, email {Email}", provider, email);
                return newUser;
            }

            Log.Warning("Failed to create or retrieve social login user for provider {Provider}, email {Email}", provider, email);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error authenticating social user for provider {Provider}, email {Email}", provider, email);
            throw;
        }
    }
}
