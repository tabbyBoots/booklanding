

namespace mvcDapper3.AppCodes.AppInterface;

public interface IAuthRepo
{
    Task<Users> AuthUserAsync(string userno, string password);
    Task<bool> ChkUsernameExistAsync(string username);
    Task RegisterUserAsync(Users user, string passwordHash);
    Task<bool> ActivateUserAsync(string token);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordTokenAsync(string token, string newPasswordHash);
    Task<Users> GetSocialLoginUserAsync(string provider, string providerKey);
    Task<Users> CreateSocialLoginUserAsync(string provider, string providerKey, string providerDisplayName, string email);
}
