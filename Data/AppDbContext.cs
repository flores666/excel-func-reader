using ExcelFuncReader.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace ExcelFuncReader.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<FunctionRecord> FunctionRecords => Set<FunctionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportJob>().ToTable("import_jobs", "func_reader");
        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.HasKey(job => job.Id);
            entity.Property(job => job.FileName).HasMaxLength(512).IsRequired();
            entity.Property(job => job.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(job => job.ErrorMessage).HasMaxLength(2048);

            entity.HasMany(x => x.FunctionRecords)
                .WithOne(x => x.ImportJob)
                .HasForeignKey(x => x.ImportJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FunctionRecord>(entity =>
        {
            entity.ToTable("functions", "func_reader");
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => record.ImportJobId);
            entity.Property(record => record.RowId).HasMaxLength(256);
            entity.Property(record => record.OrganizationName).HasMaxLength(512).IsRequired();
            entity.Property(record => record.OrganizationCode).HasMaxLength(256).IsRequired();
            entity.Property(record => record.StructuralUnitName).HasMaxLength(512).IsRequired();
            entity.Property(record => record.CodeStructuralUnit).HasMaxLength(256).IsRequired();
            entity.Property(record => record.CodeParentDivision).HasMaxLength(256);
            entity.Property(record => record.FunctionCode).HasMaxLength(128).IsRequired();
            entity.Property(record => record.FunctionDescription).HasMaxLength(2048).IsRequired();
        });
        
        modelBuilder.Entity<ImportCursor>(e =>
        {
            e.ToTable("import_cursors", "func_reader");
            e.HasKey(x => x.Id);
            e.Property(x => x.LastValue).HasMaxLength(128);
            e.HasIndex();
        });
    }
}