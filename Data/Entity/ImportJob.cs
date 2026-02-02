namespace ExcelFuncReader.Data.Entity;

public class ImportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public ImportStatus Status { get; set; } = ImportStatus.Processing;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int TotalRows { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<FunctionRecord> FunctionRecords { get; set; } = new List<FunctionRecord>();
}

public enum ImportStatus
{
    Processing,
    Completed,
    Failed
}
