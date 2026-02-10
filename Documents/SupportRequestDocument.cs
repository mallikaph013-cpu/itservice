
using myapp.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;

namespace myapp.Documents
{
    public class SupportRequestDocument : IDocument
    {
        private readonly SupportRequest _request;

        public SupportRequestDocument(SupportRequest request)
        {
            _request = request;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Content().Text($"Support Request: {_request.Id}");
                // Add more content here based on the request
            });
        }
        
        public byte[] GeneratePdf()
        {
            return Document.Create(container => Compose(container)).GeneratePdf();
        }
    }
}
