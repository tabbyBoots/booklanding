
using System.ComponentModel.DataAnnotations.Schema;

namespace mvcDapper3.Models;

public partial class Users
{
    public int Id { get; set; }

    public bool IsValid { get; set; }

    public string? UserNo { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? CodeNo { get; set; }

    public string? RoleNo { get; set; }

    public string? GenderCode { get; set; }

    [NotMapped]
    public string? GenderName { get; set; }

    public string? DeptNo { get; set; }

    public string? TitleNo { get; set; }

    public DateTime? Birthday { get; set; }

    public DateTime? OnboardDate { get; set; }

    public DateTime? LeaveDate { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactTel { get; set; }

    public string? ContactAddress { get; set; }

    public string? ValidateCode { get; set; }

    public string? NotifyPassword { get; set; }

    public string? Remark { get; set; }

    public string? ActivationToken { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public string? Settings { get; set; }

    public string? CalendarPreference { get; set; }
}

// DB first
// generate by this command: 只轉換 dbo.Users 這個表
// dotnet ef dbcontext scaffold "Name=ConnectionStrings:dbconn" Microsoft.EntityFrameworkCore.SqlServer -n mvcDapper.Models -o Models/Tables --context-dir Models -c dbEntities -f --use-database-names --no-pluralize --table dbo.Users
