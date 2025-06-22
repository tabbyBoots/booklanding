using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.Models.ViewModel;

public class vmSocialLogin
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string ErrorDescription { get; set; } = string.Empty;
}
