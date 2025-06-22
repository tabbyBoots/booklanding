using mvcDapper3.AppCodes.AppConfig;
using mvcDapper3.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using X.PagedList;
using mvcDapper3.AppCodes.AppBase;
using mvcDapper3.AppCodes.AppInterface;
using mvcDapper3.AppCodes.AppService;
using Microsoft.AspNetCore.Authorization;

namespace mvcDapper3.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Mis,User")]
public class UserController : BaseController
{
    private readonly IUserService _service;
    private readonly IConfiguration _config;

    public UserController(
        IUserService service,
        IConfiguration config)
    {
        _service = service;
        _config = config;
    }

    private async Task PopulateDropdownData()
    {
        ViewBag.GenderList = await _service.GetGenderDropdownAsync();
        ViewBag.DeptList = await _service.GetDepartmentDropdownAsync();
        ViewBag.TitleList = await _service.GetTitleDropdownAsync();
        
        // Add role dropdown for Admin/Mis users only
        if (User.IsInRole("Admin") || User.IsInRole("Mis"))
        {
            ViewBag.RoleList = await _service.GetRoleDropdownAsync();
        }
    }

    [HttpGet]
    public async Task<ActionResult> Index(int id = 1)
    {
        // Determine current user's role for filtering
        string currentUserRole = "";
        if (User.IsInRole("Admin"))
        {
            currentUserRole = "Admin";
        }
        else if (User.IsInRole("Mis"))
        {
            currentUserRole = "Mis";
        }
        else if (User.IsInRole("User"))
        {
            currentUserRole = "User";
        }

        // Get filtered data based on user role
        var data = await _service.GetAllVmByRoleAsync(currentUserRole);
        int pageSize = 10;
        var pagedData = data.ToPagedList(id, pageSize);
        ViewBag.PageInfo = $"第 {id} 頁，共 {pagedData.PageCount} 頁";
        return View(pagedData);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownData();
        return View("UserForm", new vmUserForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(vmUserForm vmUserForm)
    {
        if (vmUserForm == null)
        {
            TempData["ErrorMessage"] = "An error occurred during creation.";
            return BadRequest("資料不能為空");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownData();
            return View("UserForm", vmUserForm);
        }

        if (vmUserForm.Birthday.HasValue)
        {
            vmUserForm.Birthday = vmUserForm.Birthday.Value.Date;
        }

        if (vmUserForm.OnboardDate.HasValue)
        {
            vmUserForm.OnboardDate = vmUserForm.OnboardDate.Value.Date;
        }

        await _service.AddAsync(vmUserForm);
        TempData["SuccessMessage"] = "Record created successfully.";
        return RedirectToAction("Index", "User", new { area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int page = 1)
    {
        // Basic logging
        Log.Information("Editing user with ID: {Id}", id);
        
        var user = await _service.GetUserForEditAsync(id);
        if (user == null)
        {
            Log.Warning("User with ID {Id} not found for edit", id);
            TempData["ErrorMessage"] = "Unable to find this user. Update failed";
            return NotFound("沒有找到任何資料");
        }
        
        // Log basic user details
        Log.Information("User found: {UserName}, {UserNo}, {Email}", user.UserName, user.UserNo, user.ContactEmail);
        
        ViewBag.CurrentPage = page;
        await PopulateDropdownData();
        return View("UserForm", user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(vmUserForm vmUserForm, int currentPage = 1)
    {
        if (vmUserForm == null || !vmUserForm.Id.HasValue)
        {
            TempData["ErrorMessage"] = "An error occurred during update.";
            return BadRequest("資料不能為空");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownData();
            return View("UserForm", vmUserForm);
        }

        if (vmUserForm.Birthday.HasValue)
        {
            vmUserForm.Birthday = vmUserForm.Birthday.Value.Date;
        }

        if (vmUserForm.OnboardDate.HasValue)
        {
            vmUserForm.OnboardDate = vmUserForm.OnboardDate.Value.Date;
        }

        await _service.EditUserAsync(vmUserForm);
        TempData["SuccessMessage"] = "Record updated successfully.";
        return RedirectToAction("Index", "User", new { area = "Admin", id = currentPage });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "Delete success";
            return RedirectToAction("Index", "User", new { area = "Admin" });
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "Unable to delete";
            Log.Warning(e, "Delete failed: {Id}", id);
            return NotFound(new ProblemDetails
            {
                Title = "Resource not found",
                Detail = e.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}
