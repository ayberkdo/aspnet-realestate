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
            CreateMap<AmenitiesGroup, AmenitiesGroupViewModel>().ReverseMap();
            CreateMap<Amenities, AmenitiesViewModel>().ReverseMap();
            CreateMap<Categories, CategoryViewModel>().ReverseMap();
            CreateMap<CategoryFields, CategoryFieldsViewModel>().ReverseMap();
            CreateMap<Property, PropertyViewModel>().ReverseMap();
            CreateMap<PropertyImage, PropertyImageViewModel>().ReverseMap();
            CreateMap<PropertyDestination, PropertyDestinationViewModel>().ReverseMap();
            CreateMap<Messages, MessagesViewModel>().ReverseMap();
            CreateMap<Setting, SettingViewModel>().ReverseMap();
        }
    }
}
