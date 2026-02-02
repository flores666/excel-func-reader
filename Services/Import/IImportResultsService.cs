using ExcelFuncReader.Models;

namespace ExcelFuncReader.Services.Import;

public interface IImportResultsService
{
    Task<ImportResultsDto?> GetResultsAsync(Guid importJobId, CancellationToken ct = default);
}
