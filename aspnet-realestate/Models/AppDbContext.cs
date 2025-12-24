using aspnet_realestate.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace aspnet_realestate.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // (DbSet<User>
        public DbSet<AmenitiesGroup> AmenitiesGroups { get; set; }
        public DbSet<Amenities> Amenities { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<CategoryFields> CategoryFields { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyDestination> PropertyDestinations { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Setting> Setting { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);


            // AppUser-Message : 1 mesaj 1 kullanıcıya aittir; kullanıcı silinirse mesaj silinmez (Restrict)
            modelBuilder.Entity<Messages>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Property-Message : 1 mesaj 1 ilana aittir; ilan silinirse mesajlar da silinsin (Cascade)
            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Property)
                .WithMany(p => p.Messages)
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Message-RepliedMessage : Yanıtlanan mesaj silinse bile cevap mesajı silinmez
            modelBuilder.Entity<Messages>()
                .HasOne(m => m.RepliedMessage)
                .WithMany()
                .HasForeignKey(m => m.RepliedMessageId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}