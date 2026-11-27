
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
                .HasIndex(s => new { s.ProfessionalId, s.StartDate, s.EndDate })
                .IsUnique();
            modelBuilder.Entity<Professional>().HasIndex(p => p.Email).IsUnique();
            modelBuilder.Entity<Professional>().HasIndex(p => p.PhoneNumber).IsUnique();

            // varchar em vez de longtext: o MySQL não indexa longtext sem prefixo.
            // A unicidade do nome segue a collation da coluna (no padrão do MySQL 8, ignora
            // maiúsculas e acentos: "Corte" e "córte" são considerados iguais).
            modelBuilder.Entity<Service>().Property(s => s.Name).HasMaxLength(Service.NameMaxLength);
            modelBuilder.Entity<Service>().Property(s => s.Description).HasMaxLength(Service.DescriptionMaxLength);
            modelBuilder.Entity<Service>().HasIndex(s => s.Name).IsUnique();

            // Exclusão lógica: profissionais desativados somem das consultas.
            // Use IgnoreQueryFilters() quando o histórico for necessário.
            // Como Professional é o lado obrigatório de OpeningHours e ProfessionalServiceOffering,
            // o EF registra um aviso de modelo (filtro em relação obrigatória). Consultas com Include de
            // Professional não retornam registros de profissionais desativados (comportamento desejado).
            modelBuilder.Entity<Professional>().HasQueryFilter(p => p.DeletedAt == null);

            // Evita que a exclusão física de um profissional apague horários e ofertas
            // (e, por consequência, agendamentos) em cascata.
            modelBuilder.Entity<OpeningHours>()
                .HasOne(o => o.Professional)
                .WithMany()
                .HasForeignKey(o => o.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProfessionalServiceOffering>()
                .HasOne(o => o.Professional)
                .WithMany()
                .HasForeignKey(o => o.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
