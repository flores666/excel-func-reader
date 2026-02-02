namespace ExcelFuncReader.Data.Entity;

public sealed class ImportCursor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? LastValue { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
