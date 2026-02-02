using ExcelFuncReader.Data;
using ExcelFuncReader.Models;

namespace ExcelFuncReader.Services;

public sealed class ImportService : IImportService
{
    private readonly AppDbContext _db;
    private readonly ExcelParser _parser;
    private readonly ILogger<ImportService> _logger;

    public ImportService(AppDbContext db, ExcelParser parser, ILogger<ImportService> logger)
    {
        _db = db;
        _parser = parser;
        _logger = logger;
    }

    public async Task<Guid> ImportAsync(IFormFile file, CancellationToken ct = default)
    {
        ValidateFile(file);

        var importJob = new ImportJob
        {
            FileName = file.FileName,
            Status = ImportStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.ImportJobs.Add(importJob);
        await _db.SaveChangesAsync(ct);

        try
        {
            await using var stream = file.OpenReadStream();

            var records = _parser.Parse(stream, importJob.Id);

            _db.FunctionRecords.AddRange(records);

            importJob.Status = ImportStatus.Completed;
            importJob.CompletedAt = DateTimeOffset.UtcNow;
            importJob.TotalRows = records.Count;

            await _db.SaveChangesAsync(ct);

            return importJob.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed for file {FileName}. JobId={JobId}", file.FileName, importJob.Id);

            importJob.Status = ImportStatus.Failed;
            importJob.ErrorMessage = ex.Message;
            importJob.CompletedAt = DateTimeOffset.UtcNow;

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Failed to persist failed status for JobId={JobId}", importJob.Id);
            }

            throw;
        }
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (file.Length == 0)
            throw new InvalidOperationException("File is empty.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".xlsx" and not ".xls")
            throw new InvalidOperationException("Only .xlsx or .xls files are supported.");
    }
}
