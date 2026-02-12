using myapp.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;

namespace myapp.Documents
{
    public class ReportDocument : IDocument
    {
        private readonly ReportViewModel _reportViewModel;

        public ReportDocument(ReportViewModel reportViewModel)
        {
            _reportViewModel = reportViewModel;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                // Set the default font for the entire document
                page.DefaultTextStyle(x => x.FontFamily("Sarabun").FontSize(14));

                page.Header().Text("Support Requests Report");

                page.Content().Column(column =>
                {
                    foreach (var item in _reportViewModel.ReportItems)
                    {
                        column.Item().Text($"Request: {item.Request.Id}");
                        column.Item().Text($"Requester: {item.Request.RequesterName}");
                        column.Item().Text($"Status: {item.Request.Status}");
                        column.Item().Text("-------------------");
                    }
                });

                page.Footer().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        }

        public byte[] GeneratePdf()
        {
            return Document.Create(container => Compose(container)).GeneratePdf();
        }
    }
}
