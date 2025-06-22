using mvcDapper3.Models.ViewModel;
namespace mvcDapper3.AppCodes.AppInterface
{
    public interface IUserService
    {
        Task AddAsync(vmUserForm vmUserForm);
        Task<IEnumerable<vmUser>> GetAllVmAsync();
        Task<IEnumerable<vmUser>> GetAllVmByRoleAsync(string currentUserRole);
        Task<vmUser?> GetUserByIdAsync(int id);
        Task<vmUserForm?> GetUserForEditAsync(int id);
        Task EditUserAsync(vmUserForm vmUserForm);
        Task DeleteUserAsync(int id);

        // Task AddAsync(vmUser vmUser);
        // Task AddAsync(vmUserCreate vmUserCreate);
        // Task EditUserAsync(vmUser vmUser);

        Task<IEnumerable<SelectListItem>> GetGenderDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetDepartmentDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetTitleDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetRoleDropdownAsync();
    }
}
