namespace ExcelFuncReader.Models;

public class FunctionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ImportJobId { get; set; }
    public ImportJob? ImportJob { get; set; }

    public int RowNumber { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationCode { get; set; } = string.Empty;
    public string StructuralUnitName { get; set; } = string.Empty;
    public string CodeStructuralUnit { get; set; } = string.Empty;
    public string FunctionCode { get; set; } = string.Empty;
    public string FunctionDescription { get; set; } = string.Empty;
}
