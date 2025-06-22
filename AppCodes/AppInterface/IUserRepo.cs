using mvcDapper3.Models.ViewModel;

namespace mvcDapper3.AppCodes.AppInterface
{
    /// <summary>
    /// UserRepo介面
    /// 定義給Users類別的CRUD方法
    /// 不包含連線設定，這個在BaseRepo做
    /// </summary>
    public interface IUserRepo : IBaseRepo<Users, int>
    {
        // Task AddAsync(vmUser vmUser);
        // Task AddAsync(vmUserCreate vmUserCreate); // New method

        Task<vmUser?> GetUserByIdAsync(int id);
        Task<vmUserForm?> GetUserForEditAsync(int id);
        Task EditUserAsync(Users user);

        Task DeleteUserAsync(int id);
        new Task<IEnumerable<vmUser>> GetAllAsync();
        Task<IEnumerable<vmUser>> GetAllByRoleFilterAsync(string currentUserRole);

        Task<IEnumerable<SelectListItem>> GetGenderDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetDepartmentDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetTitleDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetRoleDropdownAsync();
    }
}
