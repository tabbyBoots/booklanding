
namespace mvcDapper3.Models.ViewModel;

/// <summary>
/// 登入表單 ViewModel 視圖模型
/// </summary>
public class vmLogin
{
    [Required(ErrorMessage = "請輸入帳號")]
    [Display(Name = "登入帳號")]
    public string UserNo { get; set; }

    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "登入密碼")]
    public string Password { get; set; }

    [Required(ErrorMessage = "請輸入驗證碼")]
    [Display(Name = "驗證碼")]
    public string CaptchaCode { get; set; }

    [Display(Name = "記住我")]
    public bool RememberMe { get; set; }
}

