namespace mvcDapper3.Models.ViewModel
{
    public class vmUserForm
    {
        [Key]
        [Display(Name = "ID")]
        public int? Id { get; set; } // Nullable for Create operations

        [Display(Name = "合法")]
        public bool IsValid { get; set; }

        [Display(Name = "帳號")]
        [Required(ErrorMessage = "使用者編號不可空白!!")]
        public string? UserNo { get; set; }

        [Display(Name = "姓名")]
        [Required(ErrorMessage = "使用者姓名不可空白!!")]
        public string? UserName { get; set; }

        [Display(Name = "性別代碼")]
        public string? GenderCode { get; set; }

        [Display(Name = "性別")]
        public string? GenderName { get; set; }

        [Display(Name = "部門代號")]
        public string? DeptNo { get; set; }

        [Display(Name = "部門")]
        public string? DeptName { get; set; }

        [Display(Name = "職稱代碼")]
        public string? TitleNo { get; set; }

        [Display(Name = "職稱")]
        public string? TitleName { get; set; }

        [Display(Name = "生日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        [Display(Name = "到職日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? OnboardDate { get; set; }

        [Display(Name = "離職日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LeaveDate { get; set; }

        [Display(Name = "電子信箱")]
        [Required(ErrorMessage = "信箱不可空白!!")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確!!")]
        public string? ContactEmail { get; set; }

        [Display(Name = "電話")]
        public string? ContactTel { get; set; }

        [Display(Name = "地址")]
        public string? ContactAddress { get; set; }

        [Display(Name = "備註")]
        public string? Remark { get; set; }

        [Display(Name = "角色代碼")]
        public string? RoleNo { get; set; }

        [Display(Name = "角色名稱")]
        public string? RoleName { get; set; }

        [Display(Name = "來源")]
        public string? CodeNo { get; set; }

        // Helper property to determine if this is a create or edit operation
        // Removed NotMapped attribute as it's not needed in a view model
        public bool IsNewRecord => !Id.HasValue || Id == 0;
    }
}
