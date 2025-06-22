using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.Models.ViewModel
{
    /// <summary>
    /// 註冊表單 ViewModel 視圖模型
    /// </summary>
    public class vmRegister
    {
        [Required(ErrorMessage = "姓名為必填")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "姓名長度需在3到50個字之間")]
        public string Username { get; set; }

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "密碼長度需為6到50個字")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "確認密碼為必填")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼不一致")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件")]
        public string Email { get; set; }

        [Required(ErrorMessage = "請選擇用戶類型")]
        public string RoleNo { get; set; } = "Member"; // Default value

        [Required(ErrorMessage = "驗證碼為必填")]
        public string CaptchaCode { get; set; }
    }
}
