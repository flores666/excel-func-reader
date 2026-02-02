namespace ExcelFuncReader.Services;

public interface IImportService
{
    public Task<Guid> ImportAsync(IFormFile file, CancellationToken ct = default);
}
