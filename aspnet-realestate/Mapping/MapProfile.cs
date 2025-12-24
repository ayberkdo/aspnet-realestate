using aspnet_realestate.Models;
using aspnet_realestate.ViewModels;
using AutoMapper;

namespace aspnet_realestate.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<AppUser, UserViewModel>().ReverseMap();
            CreateMap<AppUser, RegisterViewModel>().ReverseMap();
            CreateMap<AppUser, UserProfileViewModel>().ReverseMap();
            CreateMap<AmenitiesGroup, AmenitiesGroupViewModel>().ReverseMap();
            CreateMap<Amenities, AmenitiesViewModel>().ReverseMap();
            CreateMap<Categories, CategoryViewModel>().ReverseMap();
            CreateMap<CategoryFields, CategoryFieldsViewModel>().ReverseMap();
            CreateMap<Property, PropertyViewModel>()
                .ForMember(dest => dest.ApprovalUserId, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.PropertyDestination.City))
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.PropertyDestination.District))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.PropertyDestination.Country));
            CreateMap<PropertyImage, PropertyImageViewModel>().ReverseMap();
            CreateMap<PropertyDestination, PropertyDestinationViewModel>().ReverseMap();
            CreateMap<Messages, MessagesViewModel>().ReverseMap();
            CreateMap<Setting, SettingViewModel>().ReverseMap();
        }
    }
}
