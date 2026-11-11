using AnseremTest.Models;
using Microsoft.EntityFrameworkCore;

namespace AnseremTest.DataBase
{
    public class AppDbContext : DbContext
    {
        public DbSet<Domain> Domains { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            // ---- StopList ----
            modelBuilder.Entity<Domain>()
                .HasMany(d => d.StopList)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "DomainStopList",
                    j => j.HasOne<Address>().WithMany()
                        .HasForeignKey("StopAddressName")
                        .HasPrincipalKey(a => a.Name),
                    j => j.HasOne<Domain>().WithMany()
                        .HasForeignKey("DomainName")
                        .HasPrincipalKey(d => d.Name),
                    j =>
                    {
                        j.HasKey("DomainName", "StopAddressName");
                        j.ToTable("DomainStopList");
                    });

            // ---- SwitchList ----
            modelBuilder.Entity<Domain>()
                .HasMany(d => d.SwitchList)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "DomainSwitchList",
                    j => j.HasOne<Address>().WithMany()
                        .HasForeignKey("SwitchAddressName")
                        .HasPrincipalKey(a => a.Name),
                    j => j.HasOne<Domain>().WithMany()
                        .HasForeignKey("DomainName")
                        .HasPrincipalKey(d => d.Name),
                    j =>
                    {
                        j.HasKey("DomainName", "SwitchAddressName");
                        j.ToTable("DomainSwitchList");
                    });
        }
    }
}
