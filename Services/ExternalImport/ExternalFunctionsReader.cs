using ExcelFuncReader.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ExcelFuncReader.Services.ExternalImport;

public interface IExternalFunctionsReader
{
    Task<IReadOnlyList<ExternalFunctionRow>> ReadBatchAsync(string? lastRowId, int batchSize, CancellationToken ct);
}

public sealed class ExternalFunctionsReader : IExternalFunctionsReader
{
    private readonly string _connString;
    private readonly ExternalImportOptions _opt;

    public ExternalFunctionsReader(IConfiguration cfg, IOptions<ExternalImportOptions> opt)
    {
        _connString = cfg.GetConnectionString("RsmvConnection")
                      ?? throw new InvalidOperationException("ConnectionStrings:RsmvConnection is missing.");
        _opt = opt.Value;
    }

    public async Task<IReadOnlyList<ExternalFunctionRow>> ReadBatchAsync(string? lastRowId, int batchSize, CancellationToken ct)
    {
        var sql = $@"
SELECT
  row_id,
  organization_name,
  organization_code,
  structural_unit_name,
  code_structural_unit,
  code_parent_division,
  function_code,
  function_description,
  row_number
FROM {_opt.SourceTable}
WHERE (@last IS NULL OR row_id > @last)
ORDER BY row_id
LIMIT @limit;";

        var rows = new List<ExternalFunctionRow>(capacity: Math.Min(batchSize, 4096));

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("last", (object?)lastRowId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("limit", batchSize);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new ExternalFunctionRow(
                RowId: reader.GetString(0),
                OrganizationName: reader.GetString(1),
                OrganizationCode: reader.GetString(2),
                StructuralUnitName: reader.GetString(3),
                CodeStructuralUnit: reader.GetString(4),
                CodeParentDivision: reader.GetString(5),
                FunctionCode: reader.GetString(6),
                FunctionDescription: reader.GetString(7),
                RowNumber: reader.GetInt32(8)
            ));
        }

        return rows;
    }
}