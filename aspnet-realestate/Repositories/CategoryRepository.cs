using AutoMapper;
using aspnet_realestate.Models;
using aspnet_realestate.ViewModels;

namespace aspnet_realestate.Repositories
{
    public class CategoryRepository : GenericRepository<Categories>
    {
        public CategoryRepository(AppDbContext context) : base(context, context.Set<Categories>())
        {
        }
    }
}
