using aspnet_realestate.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace aspnet_realestate.Repositories
{
    public class PropertyRepository : GenericRepository<Property>
    {
        public PropertyRepository(AppDbContext context) : base(context, context.Set<Property>())
        {
        }

        /// Tüm ilanları kategori, konum ve görsellerle birlikte getirir.
        public async Task<List<Property>> GetAllWithIncludesAsync()
        {
            return await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.PropertyDestination)
                .Include(p => p.Images)
                .OrderByDescending(p => p.Created)
                .ToListAsync();
        }

        /// Slug veya başka koşulların var olup olmadığını kontrol eder.
        public async Task<bool> AnyAsync(Expression<Func<Property, bool>> predicate)
        {
            return await _context.Properties.AnyAsync(predicate);
        }

        /// Ülke, şehir, ilçe bilgisine göre mevcut bir PropertyDestination var mı kontrol eder.
        public async Task<PropertyDestination?> GetDestinationAsync(string country, string city, string district)
        {
            return await _context.PropertyDestinations
                .FirstOrDefaultAsync(d =>
                    d.Country.ToLower() == country.ToLower() &&
                    d.City.ToLower() == city.ToLower() &&
                    d.District.ToLower() == district.ToLower());
        }

        /// Yeni bir PropertyDestination kaydı oluşturur.
        public async Task AddDestinationAsync(PropertyDestination destination)
        {
            await _context.PropertyDestinations.AddAsync(destination);
            await _context.SaveChangesAsync();
        }

        /// İlana ait yeni bir görsel ekler.
        public async Task AddImageAsync(PropertyImage image)
        {
            await _context.PropertyImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task<Property?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.PropertyDestination)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task ClearImagesAsync(int id)
        {
            var images = await _context.PropertyImages
                .Where(img => img.PropertyId == id)
                .ToListAsync();

            if (images.Any())
            {
                _context.PropertyImages.RemoveRange(images);
                await _context.SaveChangesAsync();
            }
        }

        // 🔹 Bir destinasyon başka property tarafından kullanılıyor mu kontrol et
        public async Task<bool> IsDestinationUsedByOthers(int destinationId)
        {
            return await _context.Properties
                .AnyAsync(p => p.PropertyDestinationId == destinationId);
        }

        // 🔹 Destinasyonu sil
        public async Task DeleteDestinationAsync(PropertyDestination destination)
        {
            if (destination == null) return;

            _context.PropertyDestinations.Remove(destination);
            await _context.SaveChangesAsync();
        }

        public async Task<Property> GetPropertyBySlugAsync(string slug)
        {
            return await _context.Properties
                .Include(p => p.Messages)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }
    }
}
