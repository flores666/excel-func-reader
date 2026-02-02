using System.Text.Json.Serialization;
using ExcelFuncReader.Models;

namespace ExcelFuncReader.Data;

public class FunctionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ImportJobId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public ImportJob? ImportJob { get; set; }

    public int RowNumber { get; set; }
    public string RowId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationCode { get; set; } = string.Empty;
    public string StructuralUnitName { get; set; } = string.Empty;
    public string CodeStructuralUnit { get; set; } = string.Empty;
    public string CodeParentDivision { get; set; } = string.Empty;
    public string FunctionCode { get; set; } = string.Empty;
    public string FunctionDescription { get; set; } = string.Empty;
}
