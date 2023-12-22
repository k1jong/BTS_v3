using Microsoft.EntityFrameworkCore;

namespace BTS_v3.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<WMAHUData> Raw_WMAHUData { get; set; }
        public DbSet<KepcoDayLpData> Raw_KepcoDayLpData { get; set; }
        public DbSet<Infofare> INFO_FARE { get; set; }
        public DbSet<Rasbts> RaseberryPi { get; set; }
        public DbSet<WMINVData> Raw_WMINVData { get; set; }
    }
}
