using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Common.Utilities;

public class PdfReportGenerator
{
    private string WebRootPath { get; }

    public PdfReportGenerator(IWebHostEnvironment webHostEnvironment)
    {
        WebRootPath = webHostEnvironment.WebRootPath;
    }

    public byte[] GeneratePdfReport<T>(string title, List<T> items, List<PdfTableColumn> columns, PdfSettings? pdfSettings = null)
    {
        var document = new PdfDocument<T>(title, items, columns, WebRootPath, pdfSettings ?? new PdfSettings());

        return document.GeneratePdf();
    }

    private class PdfDocument<T> : IDocument
    {
        private string Title { get; }
        private List<T> Items { get; }
        private List<PdfTableColumn> Columns { get; }
        private string WebRootPath { get; }
        private PdfSettings Settings { get; }


        public PdfDocument(string title, List<T> items, List<PdfTableColumn> columns, string webRootPath, PdfSettings pdfSettings)
        {
            Title = title;
            Items = items;
            Columns = columns;
            WebRootPath = webRootPath;
            Settings = pdfSettings;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(GetPageSize());
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x.FontFamily("ElMessiri").FontColor(Colors.White));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignLeft().PaddingLeft(15).PaddingBottom(15).Text(x =>
                {
                    x.DefaultTextStyle(textStyle => textStyle.FontColor("#788BAA").FontSize(9));
                    x.Span($"{Title} - ");
                    x.Span("الصفحة ");
                    x.CurrentPageNumber();
                    x.Span(" من ");
                    x.TotalPages();
                });
            });
        }
        
        private PageSize GetPageSize()
        {
            var baseSize = Settings.Size switch
            {
                PdfSize.A3 => PageSizes.A3,
                PdfSize.A4 => PageSizes.A4,
                PdfSize.A5 => PageSizes.A5,
                PdfSize.Letter => PageSizes.Letter,
                _ => PageSizes.A4
            };

            return Settings.Orientation == PdfOrientation.Portrait 
                ? baseSize.Portrait() 
                : baseSize.Landscape();
        }

        private void ComposeHeader(IContainer container)
        {
            var imagePath = Path.Combine(WebRootPath, "assets", "qalaat.png");
            var imageBytes = File.ReadAllBytes(imagePath);

            container.Background("#006A64").Row(row =>
            {
                row.ConstantItem(20)
                    .Height(140);

                row.ConstantItem(120)
                    .PaddingTop(15)
                    .Height(110)
                    .Image(imageBytes);

                row.RelativeItem().PaddingRight(30).PaddingTop(10).AlignMiddle().Column(column =>
                {
                    column.Spacing(20);

                    column.Item()
                        .Text(Title)
                        .AlignCenter()
                        .FontSize(20)
                        .SemiBold();

                    column.Item().AlignCenter().Row(innerRow =>
                    {
                        innerRow.RelativeItem().Text(text =>
                        {
                            text.Span("تاريخ إعداد التقرير: ");
                            text.Span($"\u202D{DateTime.Now:yyyy-MM-dd}\u202C"); // Left-To-Right character unicode
                        });
                        
                        innerRow.RelativeItem().Text(text =>
                        {
                            text.Span("ساعة إعداد التقرير: ");
                            text.Span($"{DateTime.Now:HH:mm:ss}");
                        });
                    });
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingHorizontal(20).DefaultTextStyle(x => x.FontColor(Colors.Black)).PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);

                column.Item().AlignCenter().Element(ComposeTable);
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(descriptor =>
                {
                    foreach (var _ in Columns) descriptor.RelativeColumn();
                });

                table.Header(header =>
                {
                    foreach (var column in Columns)
                    {
                        header.Cell().Element(HeaderCellStyle).AlignCenter().AlignMiddle().Text(column.HeaderText);
                    }

                    static IContainer HeaderCellStyle(IContainer container)
                    {
                        return container
                            .Background("#F2F4F7")
                            .DefaultTextStyle(x => x.FontSize(9).FontColor("#40424B").SemiBold())
                            .Border(0.5f)
                            .BorderColor("#E4E7EC")
                            .Height(35);
                    }
                });

                var properties = typeof(T).GetProperties();
                foreach (var item in Items)
                {
                    foreach (var column in Columns)
                    {
                        var property = properties.FirstOrDefault(x => x.Name == column.PropertyName);
                        var value = property?.GetValue(item);

                        table.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text(value?.ToString() ?? string.Empty);
                    }
                }

                static IContainer CellStyle(IContainer container)
                {
                    return container
                        .DefaultTextStyle(x => x.FontSize(9).FontColor("#40424B").SemiBold())
                        .Border(0.5f)
                        .BorderColor("#E4E7EC")
                        .Height(35);
                }
            });
        }
    }
}

public record PdfTableColumn
{
    public PdfTableColumn(string headerText, string propertyName)
    {
        HeaderText = headerText;
        PropertyName = propertyName;
    }

    public string HeaderText { get; }
    public string PropertyName { get; }
}

public enum PdfSize
{
    A3,
    A4,
    A5,
    Letter
}

public enum PdfOrientation
{
    Portrait,
    Landscape
}

public record PdfSettings
{
    public PdfSize Size { get; init; } = PdfSize.A4;
    public PdfOrientation Orientation { get; init; } = PdfOrientation.Portrait;
}
