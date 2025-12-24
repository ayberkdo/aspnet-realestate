using AutoMapper;
using aspnet_realestate.Models;
using aspnet_realestate.ViewModels;

namespace aspnet_realestate.Repositories
{
    public class AmenitiesRepository : GenericRepository<Amenities>
    {
        public AmenitiesRepository(AppDbContext context) : base(context, context.Set<Amenities>())
        {
        }
    }
}
