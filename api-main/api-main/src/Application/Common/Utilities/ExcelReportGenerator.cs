using ClosedXML.Excel;

namespace Application.Common.Utilities;

public static class ExcelReportGenerator
{
    public static byte[] GenerateExcelReport<T>(string worksheetName, List<T> items, IList<ExcelColumn> columns)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(worksheetName);

        for (var i = 0; i < columns.Count; i++)
        {
            worksheet.Row(1).Cell(i + 1).Value = columns[i].HeaderText;
        }

        var properties = typeof(T).GetProperties();

        for (var rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 2; // Start from row 2 (after headers)
            var item = items[rowIndex];

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                var property = properties.FirstOrDefault(x => x.Name == column.PropertyName);
                object? cellValue = null;

                if (property != null)
                {
                    var value = property.GetValue(item);

                    if (value is DateTime or DateTimeOffset)
                    {
                        const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                        cellValue = value switch
                        {
                            DateTime dt => dt.ToString(dateTimeFormat),
                            DateTimeOffset dto => dto.ToString(dateTimeFormat),
                            _ => value.ToString()
                        };
                    }
                    else
                        cellValue = value;
                }

                worksheet.Row(rowNumber).Cell(columnIndex + 1).Value = cellValue?.ToString() ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);

        return memoryStream.ToArray();
    }

    public static byte[] GenerateExcelReport(string worksheetName, List<Dictionary<string, object?>> items, IList<ExcelColumn> columns)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(worksheetName);

        for (var i = 0; i < columns.Count; i++)
        {
            worksheet.Row(1).Cell(i + 1).Value = columns[i].HeaderText;
        }

        for (var rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 2;
            var item = items[rowIndex];

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                item.TryGetValue(column.PropertyName, out var value);
                object? cellValue;

                if (value is DateTime or DateTimeOffset)
                {
                    const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                    cellValue = value switch
                    {
                        DateTime dt => dt.ToString(dateTimeFormat),
                        DateTimeOffset dto => dto.ToString(dateTimeFormat),
                        _ => value.ToString()
                    };
                }
                else
                    cellValue = value;

                worksheet.Row(rowNumber).Cell(columnIndex + 1).Value = cellValue?.ToString() ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);

        return memoryStream.ToArray();
    }
}

public record ExcelColumn
{
    public ExcelColumn(string headerText, string propertyName)
    {
        HeaderText = headerText;
        PropertyName = propertyName;
    }

    public string HeaderText { get; init; }
    public string PropertyName { get; init; }
}