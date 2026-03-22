using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Core.Models.ArGe;
using AydaMusavirlik.Core.Models.Audit;
using AydaMusavirlik.Core.Models.CompanyFormation;
using AydaMusavirlik.Core.Models.FinancialAnalysis;

namespace AydaMusavirlik.Infrastructure.Persistence;

/// <summary>
/// Application Database Context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Common
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<User> Users => Set<User>();

    // Accounting
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountingRecord> AccountingRecords => Set<AccountingRecord>();
    public DbSet<AccountingEntry> AccountingEntries => Set<AccountingEntry>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();

    // Payroll
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<LeaveRecord> LeaveRecords => Set<LeaveRecord>();
    public DbSet<SgkBelgeTuru> SgkBelgeTurleri => Set<SgkBelgeTuru>();
    public DbSet<KanuniKesinti> KanuniKesintiler => Set<KanuniKesinti>();
    public DbSet<Puantaj> Puantajlar => Set<Puantaj>();
    public DbSet<TahakkukDetay> TahakkukDetaylari => Set<TahakkukDetay>();

    // ArGe
    public DbSet<ArGeProject> ArGeProjects => Set<ArGeProject>();
    public DbSet<ArGeEmployee> ArGeEmployees => Set<ArGeEmployee>();

    // Audit
    public DbSet<AuditReport> AuditReports => Set<AuditReport>();

    // Company Formation
    public DbSet<CompanyFormationApplication> CompanyFormationApplications => Set<CompanyFormationApplication>();
    public DbSet<ArticlesOfAssociation> ArticlesOfAssociations => Set<ArticlesOfAssociation>();

    // Financial Analysis
    public DbSet<FinancialAnalysisReport> FinancialAnalysisReports => Set<FinancialAnalysisReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }
        }

        // Company configurations
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.TaxNumber).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TaxNumber).HasMaxLength(11);
            entity.Property(e => e.Capital).HasPrecision(18, 2);
        });

        // Account configurations
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        });

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Employee configurations
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.TcKimlikNo);
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeNumber }).IsUnique();
        });

        // PayrollRecord configurations
        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month }).IsUnique();
            entity.Property(e => e.GrossSalary).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
        });
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(SoftDeleteEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}