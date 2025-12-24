using aspnet_realestate.Models;
using aspnet_realestate.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace aspnet_realestate.Repositories
{
    public class CategoryFieldsRepository : GenericRepository<CategoryFields>
    {
        public CategoryFieldsRepository(AppDbContext context) : base(context, context.Set<CategoryFields>())
        {
        }

        public async Task<List<PropertyCustomFieldViewModel>> GetFieldsByCategoryIdAsync(int categoryId)
        {
            var fields = await _context.CategoryFields
                .Where(f => f.CategoryId == categoryId && f.IsActive)
                .ToListAsync();

            return fields.Select(f => new PropertyCustomFieldViewModel
            {
                FieldName = f.FieldName,
                Value = null // Başlangıçta boş, kullanıcı inputu alacak
            }).ToList();
        }
    }
}
