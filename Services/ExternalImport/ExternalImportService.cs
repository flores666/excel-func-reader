using ExcelFuncReader.Data;
using ExcelFuncReader.Data.Entity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace ExcelFuncReader.Services.ExternalImport;

public interface IExternalImportService
{
    public Task ImportOnceAsync(CancellationToken ct);
}

public sealed class ExternalImportService : IExternalImportService
{
    private readonly AppDbContext _db;
    private readonly IExternalFunctionsReader _reader;
    private readonly ILogger<ExternalImportService> _logger;
    private readonly ExternalImportOptions _opt;

    public ExternalImportService(
        AppDbContext db,
        IExternalFunctionsReader reader,
        IOptions<ExternalImportOptions> opt,
        ILogger<ExternalImportService> logger)
    {
        _db = db;
        _reader = reader;
        _logger = logger;
        _opt = opt.Value;
    }

    public async Task ImportOnceAsync(CancellationToken ct)
    {
        if (!_opt.Enabled) return;

        var cursor = await _db.Set<ImportCursor>()
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct);
        
        if (cursor is null)
        {
            cursor = new ImportCursor { LastValue = null, UpdatedAt = DateTimeOffset.UtcNow };
            _db.Add(cursor);
            await _db.SaveChangesAsync(ct);
        }

        var importJob = new ImportJob
        {
            FileName = $"external:{_opt.SourceTable}",
            Status = ImportStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.ImportJobs.Add(importJob);
        await _db.SaveChangesAsync(ct);

        try
        {
            var batch = await _reader.ReadBatchAsync(cursor.LastValue, _opt.BatchSize, ct);

            if (batch.Count == 0)
            {
                importJob.Status = ImportStatus.Completed;
                importJob.CompletedAt = DateTimeOffset.UtcNow;
                importJob.TotalRows = 0;
                await _db.SaveChangesAsync(ct);
                return;
            }

            var seen = new HashSet<string>();

            var records = new List<FunctionRecord>(batch.Count);
            foreach (var r in batch)
            {
                var key = $"{r.OrganizationCode}|{r.CodeStructuralUnit}|{r.FunctionCode}|{r.CodeParentDivision}";
                if (!seen.Add(key))
                    continue;

                records.Add(new FunctionRecord
                {
                    ImportJobId = importJob.Id,
                    RowNumber = r.RowNumber,
                    RowId = r.RowId,
                    OrganizationName = r.OrganizationName,
                    OrganizationCode = r.OrganizationCode,
                    StructuralUnitName = r.StructuralUnitName,
                    CodeStructuralUnit = r.CodeStructuralUnit,
                    CodeParentDivision = r.CodeParentDivision,
                    FunctionCode = r.FunctionCode,
                    FunctionDescription = r.FunctionDescription
                });
            }

            _db.FunctionRecords.AddRange(records);

            cursor.LastValue = importJob.Id.ToString();
            cursor.UpdatedAt = DateTimeOffset.UtcNow;

            importJob.Status = ImportStatus.Completed;
            importJob.CompletedAt = DateTimeOffset.UtcNow;
            importJob.TotalRows = records.Count;

            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External import failed. Table={Table}", _opt.SourceTable);

            importJob.Status = ImportStatus.Failed;
            importJob.ErrorMessage = ex.Message;
            importJob.CompletedAt = DateTimeOffset.UtcNow;

            try { await _db.SaveChangesAsync(ct); } catch { /* ignore */ }

            throw;
        }
    }
}