using ExcelFuncReader.Data;

namespace ExcelFuncReader.Models;

public sealed record ImportResultsDto(
    Guid Id,
    string FileName,
    ImportStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    int TotalRows,
    IReadOnlyList<FunctionRecordDto> Records
);