using mvcDapper3.Models.ViewModel;

namespace mvcDapper3.AppCodes.AppService;

public interface IAuthService
{
    Task<Users> AuthUserAsync(string userno, string password);
    Task<bool> ChkUsernameExistAsync(string username);
    Task<string> RegisterUserAsync(vmRegister model);
    Task<bool> ActivateUserAsync(string token);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordTokenAsync(vmResetPassword model, string token);
    Task<Users> AuthenticateSocialUserAsync(string provider, string providerKey, string providerDisplayName, string email);
}
