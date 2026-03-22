using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.VeriAnaliz.Models;

namespace AydaMusavirlik.VeriAnaliz.Data;

/// <summary>
/// Veri Analiz DbContext
/// </summary>
public class VeriAnalizDbContext : DbContext
{
    public VeriAnalizDbContext(DbContextOptions<VeriAnalizDbContext> options) : base(options)
    {
    }

    public DbSet<GelirGiderKayit> GelirGiderKayitlari => Set<GelirGiderKayit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GelirGiderKayit>(entity =>
        {
            entity.ToTable("GelirGiderKayitlari");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FirmaId);
            entity.HasIndex(e => e.Tarih);
            entity.HasIndex(e => e.Tur);
            entity.HasIndex(e => e.Kategori);

            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Kategori).HasMaxLength(100);
            entity.Property(e => e.Kasa).HasMaxLength(50);
            entity.Property(e => e.BelgeNo).HasMaxLength(50);
            entity.Property(e => e.Notlar).HasMaxLength(1000);

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}

/// <summary>
/// DbContext Factory
/// </summary>
public static class VeriAnalizDbContextFactory
{
    public static VeriAnalizDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<VeriAnalizDbContext>()
            .UseSqlite(connectionString)
            .Options;

        var context = new VeriAnalizDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
