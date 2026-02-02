namespace ExcelFuncReader.Models;

public sealed record ExternalFunctionRow(
    string RowId,
    string OrganizationName,
    string OrganizationCode,
    string StructuralUnitName,
    string CodeStructuralUnit,
    string CodeParentDivision,
    string FunctionCode,
    string FunctionDescription,
    int RowNumber
);
