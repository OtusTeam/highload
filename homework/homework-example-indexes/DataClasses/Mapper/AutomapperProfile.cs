using AutoMapper;

using OtusSocialNetwork.Database.Entities;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Requests;

namespace OtusSocialNetwork.DataClasses.Mapper;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<RegisterReq, UserEntity>()
            .ForMember(e => e.Id, opt => opt.MapFrom(r => Guid.NewGuid().ToString()))
            ;
        CreateMap<UserEntity, UserDto>();
    }
}
