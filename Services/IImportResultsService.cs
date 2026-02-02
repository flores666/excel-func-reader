using ExcelFuncReader.Models;

namespace ExcelFuncReader.Services;

public interface IImportResultsService
{
    Task<ImportResultsDto?> GetResultsAsync(Guid importJobId, CancellationToken ct = default);
}
