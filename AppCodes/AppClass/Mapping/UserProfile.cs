using AutoMapper;
using mvcDapper3.Models.ViewModel;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Entity to View Models
        CreateMap<Users, vmUser>();
        CreateMap<Users, vmUserForm>();

        // View Models to Entity
        CreateMap<vmUser, Users>();
        CreateMap<vmUserForm, Users>();
    }
}
