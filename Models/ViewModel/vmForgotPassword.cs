using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.Models.ViewModel
{
    public class vmForgotPassword
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }

        [Required(ErrorMessage = "請輸入驗證碼")]
        [Display(Name = "驗證碼")]
        public string CaptchaCode { get; set; }
    }
}
