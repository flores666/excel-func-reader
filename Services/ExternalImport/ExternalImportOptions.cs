namespace ExcelFuncReader.Services.ExternalImport;

public sealed class ExternalImportOptions
{
    public const string SectionName = "ExternalImport";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
    public string SourceTable { get; set; } = "func_reader.functions";
    public int BatchSize { get; set; } = 5000;
}
