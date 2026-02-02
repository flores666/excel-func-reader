using ExcelFuncReader.Data;
using ExcelFuncReader.Models;

namespace ExcelFuncReader.Services;

using Microsoft.EntityFrameworkCore;

public sealed class ImportResultsService : IImportResultsService
{
    private readonly AppDbContext _db;
    private readonly RedisAggregator _aggregator;

    public ImportResultsService(AppDbContext db, RedisAggregator aggregator)
    {
        _db = db;
        _aggregator = aggregator;
    }

    public async Task<ImportResultsDto?> GetResultsAsync(Guid importJobId, CancellationToken ct = default)
    {
        var job = await _db.ImportJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == importJobId, ct);

        if (job is null)
            return null;
        
        // if (job.Status != ImportStatus.Completed)
        //     throw new InvalidOperationException("Import is not completed yet.");
        
        var records = await _db.FunctionRecords
            .AsNoTracking()
            .Where(r => r.ImportJobId == importJobId)
            .OrderBy(r => r.RowNumber)
            .ToListAsync(ct);
        
        await _aggregator.CacheAggregatesAsync(records);

        var recordDtos = records.Select(r => new FunctionRecordDto(
            RowNumber: r.RowNumber,
            RowId: r.RowId,
            OrganizationName: r.OrganizationName,
            OrganizationCode: r.OrganizationCode,
            StructuralUnitName: r.StructuralUnitName,
            CodeStructuralUnit: r.CodeStructuralUnit,
            CodeParentDivision: r.CodeParentDivision,
            FunctionCode: r.FunctionCode,
            FunctionDescription: r.FunctionDescription
        )).ToList();

        return new ImportResultsDto(
            Id: job.Id,
            FileName: job.FileName,
            Status: job.Status,
            CreatedAt: job.CreatedAt,
            CompletedAt: job.CompletedAt,
            TotalRows: job.TotalRows,
            Records: recordDtos
        );
    }
}
