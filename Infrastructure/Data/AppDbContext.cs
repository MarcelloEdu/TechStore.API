using Microsoft.EntityFrameworkCore;

namespace CatalogoVendas.Api.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets aqui
    }
}
