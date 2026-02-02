namespace ExcelFuncReader.Models;

public sealed record FunctionRecordDto(
    int RowNumber,
    string RowId,
    string OrganizationName,
    string OrganizationCode,
    string StructuralUnitName,
    string CodeStructuralUnit,
    string CodeParentDivision,
    string FunctionCode,
    string FunctionDescription
);