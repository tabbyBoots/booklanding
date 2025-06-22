
using mvcDapper3.Models.ViewModel;
using mvcDapper3.AppCodes.AppBase;
using mvcDapper3.AppCodes.AppInterface;
using mvcDapper3.AppCodes.AppClass;
using mvcDapper3.Models.Tables;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mvcDapper3.AppCodes.AppRepo;

public class UserRepo : BaseRepo<Users, int>, IUserRepo
{
    // private readonly IConnFactory _conn;

    /// <summary>
    /// 建構子及使用的資料表給BaseRepo
    /// </summary>
    /// <param name="connFactory"></param>
    public UserRepo(IConnFactory connFactory) : base(connFactory, "Users") { }

    #region CRUD
    /// <summary>
    /// CRUD - Read All
    /// </summary>
    /// <returns></returns>
    public new async Task<IEnumerable<vmUser>> GetAllAsync()
    {

        var queryBuilder = new DynamicQueryBuilder()
            .Select("Users.Id, Users.IsValid, Users.UserNo, Users.UserName, Users.RoleNo, Users.CodeNo, Users.GenderCode, vi_CodeGender.CodeName AS GenderName, Users.DeptNo, Departments.DeptName, Users.TitleNo, Titles.TitleName, Users.Birthday, Users.OnboardDate, Users.LeaveDate, Users.ContactEmail, Users.ContactTel, Users.ContactAddress, Users.Remark")
            .From("Users")
            .LeftJoin("Titles", "Users.TitleNo = Titles.TitleNo")
            .LeftJoin("Departments", "Users.DeptNo = Departments.DeptNo")
            .LeftJoin("vi_CodeGender", "Users.GenderCode = vi_CodeGender.CodeNo")
            .OrderBy("Users.RoleNo, Users.Id, Users.CodeNo");

        var (query, parameters) = queryBuilder.Build();

        using (var conn = _connFactory.GetOpenConn())
        {
            return await conn.QueryAsync<vmUser>(query, parameters);
        }
    }

    /// <summary>
    /// CRUD - Read All with Role-based filtering
    /// </summary>
    /// <param name="currentUserRole"></param>
    /// <returns></returns>
    public async Task<IEnumerable<vmUser>> GetAllByRoleFilterAsync(string currentUserRole)
    {
        var queryBuilder = new DynamicQueryBuilder()
            .Select("Users.Id, Users.IsValid, Users.UserNo, Users.UserName, Users.RoleNo, Users.CodeNo, Users.GenderCode, vi_CodeGender.CodeName AS GenderName, Users.DeptNo, Departments.DeptName, Users.TitleNo, Titles.TitleName, Users.Birthday, Users.OnboardDate, Users.LeaveDate, Users.ContactEmail, Users.ContactTel, Users.ContactAddress, Users.Remark")
            .From("Users")
            .LeftJoin("Titles", "Users.TitleNo = Titles.TitleNo")
            .LeftJoin("Departments", "Users.DeptNo = Departments.DeptNo")
            .LeftJoin("vi_CodeGender", "Users.GenderCode = vi_CodeGender.CodeNo");

        // Apply role-based filtering
        switch (currentUserRole?.ToLower())
        {
            case "user":
                queryBuilder.Where("Users.RoleNo = @roleFilter")
                           .AddParameter("@roleFilter", "Member");
                break;
            case "mis":
                queryBuilder.Where("Users.RoleNo = @roleFilter")
                           .AddParameter("@roleFilter", "User");
                break;
            case "admin":
                // Admin sees all users - no filter needed
                break;
            default:
                // Default to no results for unknown roles
                queryBuilder.Where("1 = 0");
                break;
        }

        queryBuilder.OrderBy("Users.RoleNo, Users.Id, Users.CodeNo");

        var (query, parameters) = queryBuilder.Build();

        using (var conn = _connFactory.GetOpenConn())
        {
            return await conn.QueryAsync<vmUser>(query, parameters);
        }
    }

    /// <summary>
    /// CRUD - Read by Id for display
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<vmUser?> GetUserByIdAsync(int id)
    {
        var queryBuilder = new DynamicQueryBuilder()
            .Select(@"Users.Id, Users.IsValid, Users.UserNo, Users.UserName, Users.RoleNo, Users.CodeNo,
            Users.GenderCode, vi_CodeGender.CodeName AS GenderName, Users.DeptNo, 
            Departments.DeptName, Users.TitleNo, Titles.TitleName, Users.Birthday, 
            Users.OnboardDate, Users.LeaveDate, Users.ContactEmail, Users.ContactTel, 
            Users.ContactAddress, Users.Remark")
            .From("Users")
            .LeftJoin("Titles", "Users.TitleNo = Titles.TitleNo")
            .LeftJoin("Departments", "Users.DeptNo = Departments.DeptNo")
            .LeftJoin("vi_CodeGender", "Users.GenderCode = vi_CodeGender.CodeNo")
            .Where("Users.Id = @id")
            .AddParameter("@id", id)
            .OrderBy("Users.Id ASC");

        var (query, parameters) = queryBuilder.Build();

        using (var conn = _connFactory.GetOpenConn())
        {
            return await conn.QueryFirstAsync<vmUser>(query, parameters);
        }
    }

    /// <summary>
    /// CRUD - Read by Id for edit form
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<vmUserForm?> GetUserForEditAsync(int id)
    {
        // Log the ID being requested
        Log.Information("Getting user for edit with ID: {Id}", id);
        
        var queryBuilder = new DynamicQueryBuilder()
            .Select(@"Users.Id, Users.IsValid, Users.UserNo, Users.UserName, Users.RoleNo, Users.CodeNo,
            Users.GenderCode, vi_CodeGender.CodeName AS GenderName, Users.DeptNo, 
            Departments.DeptName, Users.TitleNo, Titles.TitleName, Users.Birthday, 
            Users.OnboardDate, Users.LeaveDate, Users.ContactEmail, Users.ContactTel, 
            Users.ContactAddress, Users.Remark")
            .From("Users")
            .LeftJoin("Titles", "Users.TitleNo = Titles.TitleNo")
            .LeftJoin("Departments", "Users.DeptNo = Departments.DeptNo")
            .LeftJoin("vi_CodeGender", "Users.GenderCode = vi_CodeGender.CodeNo")
            .Where("Users.Id = @id")
            .AddParameter("@id", id)
            .OrderBy("Users.Id ASC");

        var (query, parameters) = queryBuilder.Build();
        
        // Log the SQL query for debugging
        Log.Information("SQL Query for user edit: {Query}", query);
        Log.Information("Parameters: {@Parameters}", parameters);
        
        using (var conn = _connFactory.GetOpenConn())
        {
            try
            {
                // Execute the query and get the result
                var result = await conn.QueryFirstAsync<vmUserForm>(query, parameters);
                
                // Log the result
                if (result != null)
                {
                    Log.Information("User found for edit: {UserName}, {UserNo}", result.UserName, result.UserNo);
                }
                else
                {
                    Log.Warning("No user found for edit with ID: {Id}", id);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                // Log the error with more details
                Log.Error(ex, "Error retrieving user with ID {Id} for edit", id);
                return null;
            }
        }
    }

    /// <summary>
    /// CRUD - Edit
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task EditUserAsync(Users user)
    {
        await base.EditAsync(user);
    }

    /// <summary>
    /// CRUD - Delete
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteUserAsync(int id)
    {
        await base.DeleteAsync(id);
    }

    #endregion

    #region 下拉選單
    public async Task<IEnumerable<SelectListItem>> GetGenderDropdownAsync()
    {
        var query = @"SELECT 
        CodeNo as Value, 
        CodeName as Text 
        FROM vi_CodeGender 
        ORDER BY CodeNo";
        using (var conn = _connFactory.GetOpenConn())
        {
            var result = await conn.QueryAsync<SelectListItem>(query);
            return result;
        }
    }

    public async Task<IEnumerable<SelectListItem>> GetDepartmentDropdownAsync()
    {
        var query = @"SELECT 
        DeptNo as Value, 
        DeptName as Text 
        FROM Departments 
        ORDER BY DeptNo";

        using (var conn = _connFactory.GetOpenConn())
        {
            var result = await conn.QueryAsync<SelectListItem>(query);
            return result;
        }
    }

    public async Task<IEnumerable<SelectListItem>> GetTitleDropdownAsync()
    {
        var query = "SELECT TitleNo as Value, TitleName as Text FROM Titles ORDER BY TitleNo";

        using (var conn = _connFactory.GetOpenConn())
        {
            var result = await conn.QueryAsync<SelectListItem>(query);
            return result;
        }
    }

    public async Task<IEnumerable<SelectListItem>> GetRoleDropdownAsync()
    {
        // Create a static list of roles since they're predefined
        var roles = new List<SelectListItem>
        {
            new SelectListItem { Value = "Admin", Text = "管理員 (Admin)" },
            new SelectListItem { Value = "Mis", Text = "資訊人員 (Mis)" },
            new SelectListItem { Value = "User", Text = "使用者 (User)" },
            new SelectListItem { Value = "Member", Text = "會員 (Member)" },
            new SelectListItem { Value = "Vendor", Text = "供應商 (Vendor)" },
            new SelectListItem { Value = "Customer", Text = "客戶 (Customer)" }
        };

        return await Task.FromResult(roles);
    }

    #endregion

    #region 建立SQL字串
    //建立 Incert 查詢字串
    protected override string BuildInsertQuery()
    {
        return @"INSERT INTO Users (UserNo, 
        IsValid, UserName, RoleNo, CodeNo, DeptNo, 
        GenderCode, TitleNo, Birthday, OnboardDate, 
        ContactEmail, ContactTel, ContactAddress, Remark) 
        VALUES (@UserNo, @IsValid, @UserName, @RoleNo, 
        ISNULL(@CodeNo, 'Official'), @DeptNo, @GenderCode, @TitleNo, @Birthday, @OnboardDate, @ContactEmail, @ContactTel, @ContactAddress, @Remark)";
    }

    //建立 Update 查詢字串
    protected override string BuildUpdateQuery()
    {
        return @"UPDATE Users SET 
            UserNo = @UserNo, 
            IsValid = @IsValid,
            UserName = @UserName, 
            RoleNo = @RoleNo, 
            CodeNo = @CodeNo,
            DeptNo = @DeptNo,
            GenderCode = @GenderCode,
            TitleNo = @TitleNo,
            Birthday = @Birthday,
            OnboardDate = @OnboardDate,
            ContactEmail = @ContactEmail,
            ContactTel = @ContactTel,
            ContactAddress = @ContactAddress,
            Remark = @Remark
            WHERE Id = @Id";
    }

    //建立 Select 查詢字串
    protected override string BuildSelectQuery()
    {
        var queryBuilder = new DynamicQueryBuilder()
            .Select("Users.Id, Users.IsValid, Users.UserNo, Users.UserName, Users.GenderCode, vi_CodeGender.CodeName AS GenderName, Users.DeptNo, Departments.DeptName, Users.TitleNo, Titles.TitleName, Users.Birthday, Users.OnboardDate, Users.LeaveDate, Users.ContactEmail, Users.ContactTel, Users.ContactAddress, Users.Remark")
            .From("Users")
            .LeftJoin("Titles", "Users.TitleNo = Titles.TitleNo")
            .LeftJoin("Departments", "Users.DeptNo = Departments.DeptNo")
            .LeftJoin("vi_CodeGender", "Users.GenderCode = vi_CodeGender.CodeNo")
            .OrderBy("Users.Id DESC");
        // DynamicQueryBuild will append 'JOIN'. 
        // "LEFT OUTER" or "LEFT INNER" or "RIGHT" or "FULL" could be used.

        _queryParams = queryBuilder.GetParameters();
        return queryBuilder.BuildQuery();
    }

    #endregion
}
