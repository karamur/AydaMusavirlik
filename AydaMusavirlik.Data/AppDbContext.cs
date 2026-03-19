using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Core.Models.ArGe;
using AydaMusavirlik.Core.Models.Audit;
using AydaMusavirlik.Core.Models.CompanyFormation;
using AydaMusavirlik.Core.Models.FinancialAnalysis;

namespace AydaMusavirlik.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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

        // AccountingRecord configurations
        modelBuilder.Entity<AccountingRecord>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.DocumentNumber }).IsUnique();
            entity.Property(e => e.TotalDebit).HasPrecision(18, 2);
            entity.Property(e => e.TotalCredit).HasPrecision(18, 2);
        });

        // AccountingEntry configurations
        modelBuilder.Entity<AccountingEntry>(entity =>
        {
            entity.Property(e => e.Debit).HasPrecision(18, 2);
            entity.Property(e => e.Credit).HasPrecision(18, 2);
        });

        // Employee configurations
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.TcKimlikNo).IsUnique();
            entity.Property(e => e.GrossSalary).HasPrecision(18, 2);
        });

        // PayrollRecord configurations
        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.Property(e => e.GrossSalary).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.Property(e => e.SgkWorkerDeduction).HasPrecision(18, 2);
            entity.Property(e => e.SgkEmployerCost).HasPrecision(18, 2);
            entity.Property(e => e.IncomeTax).HasPrecision(18, 2);
            entity.Property(e => e.StampTax).HasPrecision(18, 2);
        });

        // ArGeProject configurations
        modelBuilder.Entity<ArGeProject>(entity =>
        {
            entity.Property(e => e.PlannedBudget).HasPrecision(18, 2);
            entity.Property(e => e.ActualCost).HasPrecision(18, 2);
        });

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(SoftDeleteEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
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

        foreach (var entry in ChangeTracker.Entries<SoftDeleteEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}