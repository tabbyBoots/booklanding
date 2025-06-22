using AutoMapper;
using mvcDapper3.Models.ViewModel;

namespace mvcDapper3.AppCodes.AppService;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepo _repo;
    public UserService(IUserRepo repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    // Provide CRUD methods
    public async Task AddAsync(vmUserForm vmUserForm)
    {
        var user = _mapper.Map<Users>(vmUserForm);
        await _repo.AddAsync(user);
    }

    public async Task<IEnumerable<vmUser>> GetAllVmAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<IEnumerable<vmUser>> GetAllVmByRoleAsync(string currentUserRole)
    {
        return await _repo.GetAllByRoleFilterAsync(currentUserRole);
    }

    public async Task<vmUser?> GetUserByIdAsync(int id)
    {
        return await _repo.GetUserByIdAsync(id);
    }

    public async Task<vmUserForm?> GetUserForEditAsync(int id)
    {
        return await _repo.GetUserForEditAsync(id);
    }

    public async Task EditUserAsync(vmUserForm vmUserForm)
    {
        var user = _mapper.Map<Users>(vmUserForm);
        await _repo.EditUserAsync(user);
    }

    public async Task DeleteUserAsync(int id)
    {
        await _repo.DeleteUserAsync(id);
    }

    public async Task<IEnumerable<SelectListItem>> GetGenderDropdownAsync()
    {
        return await _repo.GetGenderDropdownAsync();
    }

    public async Task<IEnumerable<SelectListItem>> GetDepartmentDropdownAsync()
    {
        return await _repo.GetDepartmentDropdownAsync();
    }

    public async Task<IEnumerable<SelectListItem>> GetTitleDropdownAsync()
    {
        return await _repo.GetTitleDropdownAsync();
    }

    public async Task<IEnumerable<SelectListItem>> GetRoleDropdownAsync()
    {
        return await _repo.GetRoleDropdownAsync();
    }

}
