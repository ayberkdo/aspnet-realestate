using aspnet_realestate.Models;
using aspnet_realestate.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace aspnet_realestate.Repositories
{
    public class AmenitiesGroupRepository : GenericRepository<AmenitiesGroup>
    {
        public AmenitiesGroupRepository(AppDbContext context) : base(context, context.Set<AmenitiesGroup>())
        {
        }

        public async Task<List<PropertyAmenitiesGroupViewModel>> GetAllWithAmenitiesAsync()
        {
            var groups = await _context.AmenitiesGroups
                .Include(g => g.Amenities)
                .Where(g => g.IsActive)
                .ToListAsync();

            return groups.Select(g => new PropertyAmenitiesGroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Amenities = g.Amenities.Select(a => new PropertyAmenitiesItemViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl
                }).ToList()
            }).ToList();
        }
    }
}
