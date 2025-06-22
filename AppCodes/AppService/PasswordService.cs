using System;
using System.Threading.Tasks;
using mvcDapper3.Models;
using mvcDapper3.Models.ViewModel;
using mvcDapper3.AppCodes.AppInterface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using mvcDapper3.AppCodes.AppConfig;
using Dapper;

namespace mvcDapper3.AppCodes.AppService
{
    public interface IPasswordService
    {
        Task<bool> ResetPasswordWithTokenAsync(vmResetPassword model, string token);
        Task<bool> ChangePasswordAsync(string userNo, string currentPassword, string newPassword);
        Task<bool> ValidatePasswordAsync(string userNo, string password);
        Task<string> GeneratePasswordResetTokenAsync(string email);
    }

    public partial class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;
        private readonly ICrypto _crypto;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ISessionService _sessionService;
        private readonly IWebHostEnvironment _env;

        public PasswordService(ILogger<PasswordService> logger, ICrypto crypto, EmailService emailService,
            IConfiguration config, ISessionService sessionService, IWebHostEnvironment env)
        {
            _logger = logger;
            _crypto = crypto;
            _emailService = emailService;
            _config = config;
            _sessionService = sessionService;
            _env = env;
        }

        public async Task<bool> ResetPasswordWithTokenAsync(vmResetPassword model, string token)
        {
            try
            {
                // Validate token
                var tokenInfo = await ValidateResetTokenAsync(token);
                if (tokenInfo == null || tokenInfo.IsExpired(10)) // 10 minutes expiry
                {
                    _logger.LogWarning("Invalid or expired password reset token: {Token}", token);
                    return false;
                }

                // Get user by email from token
                var user = await GetUserByEmailAsync(tokenInfo.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for password reset token: {Token}", token);
                    return false;
                }

                // Update password
                var success = await UpdateUserPasswordAsync(user.UserNo, model.Password);
                if (success)
                {
                    _logger.LogInformation("Password reset successful for user: {UserNo}", user.UserNo);
                    await InvalidateResetTokenAsync(token);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password with token: {Token}", token);
                throw new PasswordResetException($"Failed to reset password: {ex.Message}");
            }
        }

        private async Task<vmPasswordResetTokenInfo> ValidateResetTokenAsync(string token)
        {
            try
            {
                // Get database configuration
                var databaseConfig = _config
                    .GetSection(DatabaseConfig.SectionName)
                    .Get<DatabaseConfig>();

                if (databaseConfig == null)
                {
                    throw new Exception("Database configuration section is missing.");
                }

                string connStr = databaseConfig.GetSecureConnectionString(!_env.IsDevelopment());
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();

                // Query for user with matching token that hasn't expired
                var query = @"
                    SELECT ContactEmail 
                    FROM Users 
                    WHERE ActivationToken = @Token 
                      AND TokenExpiry > @CurrentTime
                      AND IsValid = 1";
                      
                var parameters = new { Token = token, CurrentTime = DateTime.UtcNow };
                
                var email = await connection.QueryFirstOrDefaultAsync<string>(query, parameters);
                
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Token validation failed: {Token}", token);
                    return null; // Token not found, expired, or invalid
                }
                
                // Create token info object with the retrieved email
                return new vmPasswordResetTokenInfo(token, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token: {Token}", token);
                return null;
            }
        }

        private async Task<Users> GetUserByEmailAsync(string email)
        {
            try
            {
                // Get database configuration
                var databaseConfig = _config
                    .GetSection(DatabaseConfig.SectionName)
                    .Get<DatabaseConfig>();

                if (databaseConfig == null)
                {
                    throw new Exception("Database configuration section is missing.");
                }

                string connStr = databaseConfig.GetSecureConnectionString(!_env.IsDevelopment());
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();

                var queryBuilder = new DynamicQueryBuilder()
                    .Select("*")
                    .From("Users")
                    .Where("ContactEmail = @Email AND IsValid = 1")
                    .AddParameter("@Email", email);

                var (query, parameters) = queryBuilder.Build();
                return await connection.QueryFirstOrDefaultAsync<Users>(query, parameters);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }

        private async Task<bool> UpdateUserPasswordAsync(string userNo, string newPassword)
        {
            try
            {
                // Get database configuration
                var databaseConfig = _config
                    .GetSection(DatabaseConfig.SectionName)
                    .Get<DatabaseConfig>();

                if (databaseConfig == null)
                {
                    throw new Exception("Database configuration section is missing.");
                }

                string connStr = databaseConfig.GetSecureConnectionString(!_env.IsDevelopment());
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();

                // Hash the password using the crypto service
                var hashedPassword = _crypto.CreatePasswordHash(newPassword);

                var query = "UPDATE Users SET Password = @Password WHERE UserNo = @UserNo AND IsValid = 1";
                var parameters = new { Password = hashedPassword, UserNo = userNo };

                var result = await connection.ExecuteAsync(query, parameters);
                return result > 0;
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user: {UserNo}", userNo);
                return false;
            }
        }

        private async Task InvalidateResetTokenAsync(string token)
        {
            try
            {
                // Remove the token from the session (for backward compatibility)
                await _sessionService.RemoveAsync($"PasswordResetToken_{token}");
                
                // Get database configuration
                var databaseConfig = _config
                    .GetSection(DatabaseConfig.SectionName)
                    .Get<DatabaseConfig>();

                if (databaseConfig == null)
                {
                    throw new Exception("Database configuration section is missing.");
                }

                string connStr = databaseConfig.GetSecureConnectionString(!_env.IsDevelopment());
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();
                
                // Clear token and expiry in database
                var query = "UPDATE Users SET ActivationToken = NULL, TokenExpiry = NULL WHERE ActivationToken = @Token";
                var parameters = new { Token = token };
                
                await connection.ExecuteAsync(query, parameters);
                
                _logger.LogInformation("Password reset token invalidated: {Token}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating password reset token: {Token}", token);
            }
        }

        public async Task<bool> ChangePasswordAsync(string userNo, string currentPassword, string newPassword)
        {
            try
            {
                // First validate current password
                var isValid = await ValidatePasswordAsync(userNo, currentPassword);
                if (!isValid)
                {
                    _logger.LogWarning("Password change failed - invalid current password for user: {UserNo}", userNo);
                    return false;
                }

                // Update to new password
                return await UpdateUserPasswordAsync(userNo, newPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserNo}", userNo);
                throw new PasswordResetException($"Failed to change password: {ex.Message}");
            }
        }

        public async Task<bool> ValidatePasswordAsync(string userNo, string password)
        {
            try
            {
                // Get database configuration
                var databaseConfig = _config
                    .GetSection(DatabaseConfig.SectionName)
                    .Get<DatabaseConfig>();

                if (databaseConfig == null)
                {
                    throw new Exception("Database configuration section is missing.");
                }

                string connStr = databaseConfig.GetSecureConnectionString(!_env.IsDevelopment());
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();

                var query = "SELECT Password FROM dbo.Users WHERE UserNo = @UserNo AND IsValid = 1";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserNo", userNo);
                    var hashedPassword = await command.ExecuteScalarAsync() as string;
                    return _crypto.VerifyPassword(password, hashedPassword);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user: {UserNo}", userNo);
                throw new PasswordResetException($"Failed to validate password: {ex.Message}");
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset token requested for non-existent email: {Email}", email);
                    return null;
                }

                var token = Guid.NewGuid().ToString();
                var tokenInfo = new vmPasswordResetTokenInfo(token, user.ContactEmail);
                
                // Store token in session with 10 minute expiry (for backward compatibility)
                await _sessionService.SetObjectAsync($"PasswordResetToken_{token}", tokenInfo, TimeSpan.FromMinutes(10));
                
                // Store token in database
                var databaseConfig = _config
                    .GetSection(DatabaseConfig.SectionName)
                    .Get<DatabaseConfig>();

                if (databaseConfig == null)
                {
                    throw new Exception("Database configuration section is missing.");
                }

                string connStr = databaseConfig.GetSecureConnectionString(!_env.IsDevelopment());
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();

                // Update user record with token and expiry
                var expiryTime = DateTime.UtcNow.AddMinutes(10);
                var updateQuery = "UPDATE Users SET ActivationToken = @Token, TokenExpiry = @Expiry WHERE ContactEmail = @Email AND IsValid = 1";
                var parameters = new { Token = token, Expiry = expiryTime, Email = email };
                
                await connection.ExecuteAsync(updateQuery, parameters);
                _logger.LogInformation("Password reset token generated for email: {Email}", email);
                
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for email: {Email}", email);
                throw new PasswordResetException($"Failed to generate reset token: {ex.Message}");
            }
        }
    }
}
