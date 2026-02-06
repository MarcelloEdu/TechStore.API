using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Category =====
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.Description)
                      .HasMaxLength(255);

                entity.HasMany(c => c.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey(p => p.CategoryId);
            });

            // ===== Product =====
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.StockQuantity)
                      .IsRequired();
            });

         }
    }
}
