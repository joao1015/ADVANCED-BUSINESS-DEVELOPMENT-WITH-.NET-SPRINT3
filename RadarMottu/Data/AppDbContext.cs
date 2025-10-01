using Microsoft.EntityFrameworkCore;
using RadarMottuAPI.Models;

namespace RadarMottuAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Anchor> Anchors => Set<Anchor>();
    public DbSet<Moto> Motos => Set<Moto>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Moto>()
            .HasOne(m => m.Tag)
            .WithOne(t => t.Moto)
            .HasForeignKey<Moto>(m => m.TagId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Uid)
            .IsUnique();
    }
}
