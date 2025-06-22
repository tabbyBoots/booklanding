using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.Models.ViewModel
{
    public class vmResetPassword
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "請輸入新密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        [StringLength(50, ErrorMessage = "{0} 長度至少必須為 {6} 個字元。", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不相符。")]
        public string ConfirmPassword { get; set; }
    }
}
