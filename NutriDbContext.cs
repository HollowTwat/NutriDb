using Microsoft.EntityFrameworkCore;
using NutriDbService.DbModels;

namespace NutriDbService
{
    public class NutriDbContext : railwayContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#if DEBUG
                optionsBuilder.UseNpgsql(Properties.Resources.DebugConnectionString);
#else
                optionsBuilder.UseNpgsql(Properties.Resources.DbConnectionString);
#endif
            }
        }
    }
}
