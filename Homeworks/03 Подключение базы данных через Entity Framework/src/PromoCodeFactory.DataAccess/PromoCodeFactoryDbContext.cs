using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess;

public class PromoCodeFactoryDbContext : DbContext
{
    public PromoCodeFactoryDbContext(DbContextOptions<PromoCodeFactoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPromoCode> CustomerPromoCodes => Set<CustomerPromoCode>();
    public DbSet<Preference> Preferences => Set<Preference>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey("RoleId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Preference>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.ServiceInfo).HasMaxLength(200);
            entity.Property(e => e.PartnerName).HasMaxLength(256);
            entity.HasOne(e => e.PartnerManager)
                .WithMany()
                .HasForeignKey("PartnerManagerId");
            entity.HasOne(e => e.Preference)
                .WithMany()
                .HasForeignKey("PreferenceId");
            entity.HasMany(e => e.CustomerPromoCodes)
                .WithOne()
                .HasForeignKey(e => e.PromoCodeId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
