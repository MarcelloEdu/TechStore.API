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
            public DbSet<Order> Orders => Set<Order>();
            public DbSet<OrderItem> OrderItems => Set<OrderItem>();

            
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // ===== Category =====
                  modelBuilder.Entity<Category>(entity =>
                        {
                        entity.HasKey(c => c.Id); //PK

                        entity.Property(c => c.Name) //precisa de um nome
                              .IsRequired()
                              .HasMaxLength(100);

                        entity.Property(c => c.Description) //descrição 
                              .HasMaxLength(255);

                        entity.HasMany(c => c.Products) //1:N
                              .WithOne(p => p.Category) //N:1
                              .HasForeignKey(p => p.CategoryId); //FK
                        });

                        // ===== Product =====
                        modelBuilder.Entity<Product>(entity =>
                        {
                        entity.HasKey(p => p.Id); //PK

                        entity.Property(p => p.Name) //precisa de um nome
                              .IsRequired()
                              .HasMaxLength(150);

                        entity.Property(p => p.Price) //tem um preço 
                              .IsRequired()
                              .HasColumnType("decimal(18,2)");

                        entity.Property(p => p.StockQuantity) //tem uma quantidade em estoque
                              .IsRequired();
                        });

                        // ===== Order =====
                        modelBuilder.Entity<Order>(entity =>
                        {
                        entity.HasKey(o => o.Id); //PK

                        entity.Property(o => o.CreatedAt)   //data de criação
                              .IsRequired();

                        entity.Property(o => o.TotalAmount)       //valor total do pedido
                              .HasColumnType("decimal(18,2)");

                        entity.Property(o => o.Status)           //status do pedido
                              .IsRequired();

                        entity.HasMany(o => o.Items)        //1:N
                              .WithOne(oi => oi.Order)      //N:1
                              .HasForeignKey(oi => oi.OrderId)    //FK
                              .OnDelete(DeleteBehavior.Cascade);  //deleta os itens quando o pedido é deletado
                        });

                        // ===== OrderItem =====
                        modelBuilder.Entity<OrderItem>(entity =>
                        {
                        entity.HasKey(oi => oi.Id);

                        entity.Property(oi => oi.UnitPrice) //preço unitário do item
                              .IsRequired()
                              .HasColumnType("decimal(18,2)");

                        entity.Property(oi => oi.Quantity)  
                              .IsRequired();
                  });
            }
      }
}
