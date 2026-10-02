
using BarberWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberWeb.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Professional> Professionals { get; set; }
        public DbSet<SchedulingHours> Appointments { get; set; }
        public DbSet<ProfessionalServiceOffering> ProfessionalOfferings { get; set; }
        public DbSet<OpeningHours> OpeningHours { get; set; }
        public DbSet<Service> Services { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SchedulingHours>()
                .HasIndex(s => new { s.StartDate, s.EndDate, s.ProfessionalId })
                .IsUnique();
        }
    }
}
