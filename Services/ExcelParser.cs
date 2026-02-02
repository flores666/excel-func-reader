using ExcelFuncReader.Models;
using NPOI.SS.UserModel;

namespace ExcelFuncReader.Services;

public class ExcelParser
{
    public List<FunctionRecord> Parse(Stream stream, Guid importJobId)
    {
        var records = new List<FunctionRecord>();
        var formatter = new DataFormatter();

        using var workbook = WorkbookFactory.Create(stream);
        var sheet = workbook.GetSheetAt(0);

        for (var rowIndex = sheet.FirstRowNum; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row is null)
            {
                continue;
            }

            var organizationName = GetCellValue(row, formatter, 0);
            var organizationCode = GetCellValue(row, formatter, 1);
            var structuralUnitName = GetCellValue(row, formatter, 2);
            var codeStructuralUnit = GetCellValue(row, formatter, 3);
            var functionCode = GetCellValue(row, formatter, 4);
            var functionDescription = GetCellValue(row, formatter, 5);

            if (IsHeaderRow(rowIndex, organizationName, structuralUnitName, functionDescription))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(organizationName)
                && string.IsNullOrWhiteSpace(organizationCode)
                && string.IsNullOrWhiteSpace(structuralUnitName)
                && string.IsNullOrWhiteSpace(codeStructuralUnit)
                && string.IsNullOrWhiteSpace(functionCode)
                && string.IsNullOrWhiteSpace(functionDescription))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(functionDescription))
            {
                continue;
            }

            records.Add(new FunctionRecord
            {
                ImportJobId = importJobId,
                RowNumber = rowIndex + 1,
                OrganizationName = organizationName,
                OrganizationCode = organizationCode,
                StructuralUnitName = structuralUnitName,
                CodeStructuralUnit = codeStructuralUnit,
                FunctionCode = functionCode,
                FunctionDescription = functionDescription
            });
        }

        return records;
    }

    private static string GetCellValue(IRow row, DataFormatter formatter, int columnIndex)
    {
        var cell = row.GetCell(columnIndex);
        return cell is null ? string.Empty : formatter.FormatCellValue(cell).Trim();
    }

    private static bool IsHeaderRow(int rowIndex, string organizationName, string structuralUnitName, string functionDescription)
    {
        if (rowIndex != 0)
        {
            return false;
        }

        return organizationName.Contains("орган", StringComparison.OrdinalIgnoreCase)
            || organizationName.Contains("organization", StringComparison.OrdinalIgnoreCase)
            || structuralUnitName.Contains("подраздел", StringComparison.OrdinalIgnoreCase)
            || functionDescription.Contains("function", StringComparison.OrdinalIgnoreCase);
    }
}
