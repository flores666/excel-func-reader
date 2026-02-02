using ExcelFuncReader.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelFuncReader.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<FunctionRecord> FunctionRecords => Set<FunctionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.HasKey(job => job.Id);
            entity.Property(job => job.FileName).HasMaxLength(512).IsRequired();
            entity.Property(job => job.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(job => job.ErrorMessage).HasMaxLength(2048);
        });

        modelBuilder.Entity<FunctionRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => record.ImportJobId);
            entity.Property(record => record.OrganizationName).HasMaxLength(512).IsRequired();
            entity.Property(record => record.OrganizationCode).HasMaxLength(64).IsRequired();
            entity.Property(record => record.StructuralUnitName).HasMaxLength(512).IsRequired();
            entity.Property(record => record.CodeStructuralUnit).HasMaxLength(64).IsRequired();
            entity.Property(record => record.FunctionCode).HasMaxLength(128).IsRequired();
            entity.Property(record => record.FunctionDescription).HasMaxLength(2048).IsRequired();
        });
    }
}
