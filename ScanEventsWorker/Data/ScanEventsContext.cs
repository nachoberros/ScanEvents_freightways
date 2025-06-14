using Microsoft.EntityFrameworkCore;
using ScanEventsWorker.Models;

namespace ScanEventsWorker.Data
{
    public class ScanEventsContext(DbContextOptions<ScanEventsContext> options) : DbContext(options)
    {
        public DbSet<Parcel> Parcels { get; set; }
        public DbSet<ScanEvent> ScanEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parcel>().HasKey(p => p.Id);
            modelBuilder.Entity<ScanEvent>().HasKey(e => e.Id);
        }
    }
}
