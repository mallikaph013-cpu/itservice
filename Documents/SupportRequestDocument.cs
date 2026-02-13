using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using myapp.Models;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace myapp.Documents
{
    public class SupportRequestDocument : IDocument
    {
        private readonly SupportRequest _request;
        private readonly List<User> _approvers;

        public SupportRequestDocument(SupportRequest request, List<User> approvers)
        {
            _request = request;
            _approvers = approvers;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily("Sarabun").FontSize(10));
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ใบแจ้งซ่อม/บริการฝ่ายเทคโนโลยีสารสนเทศ").Bold().FontSize(16);
                        col.Item().Text($"Service Request #{_request.DocumentNo}").SemiBold().FontSize(12);
                    });
                });

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(20);
                column.Item().Element(ComposeRequesterInfo);
                column.Item().Element(ComposeRequestDetails);
                column.Item().Element(ComposeApprovalSignatures);
            });
        }

        void ComposeCard(IContainer container, string title, Action<IContainer> content)
        {
            container
                .Border(1, Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().PaddingBottom(5).Text(title).SemiBold().FontSize(12);
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten3);
                    column.Item().PaddingTop(10).Element(content);
                });
        }

        void ComposeRequesterInfo(IContainer container)
        {
            ComposeCard(container, "ข้อมูลผู้แจ้ง", card =>
            {
                card.Row(row =>
                {
                    row.RelativeItem(2).Column(col =>
                    {
                        col.Item().Text(text => { text.Span("ผู้แจ้ง: ").SemiBold(); text.Span(_request.RequesterName); });
                        col.Item().Text(text => { text.Span("แผนก: ").SemiBold(); text.Span(_request.Department); });
                    });
                    row.RelativeItem(3).Column(col =>
                    {
                        col.Item().Text(text => { text.Span("ประเภทคำขอ: ").SemiBold(); text.Span(_request.RequestType.ToString()); });
                        if (_request.RequestType == RequestType.Software)
                        {
                            col.Item().Text(text => { text.Span("โปรแกรม: ").SemiBold(); text.Span(_request.Program.ToString()); });
                        }
                    });
                });
            });
        }

        void ComposeRequestDetails(IContainer container)
        {
            ComposeCard(container, "รายละเอียดคำขอ", card =>
            {
                card.Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Text(text => { text.Span("ปัญหาที่พบ: ").SemiBold(); text.Span(_request.ProblemDescription); });
                    column.Item().Text(text => { text.Span("ความเร่งด่วน: ").SemiBold(); text.Span(_request.Urgency.ToString()); });

                    if (_request.RequestType == RequestType.Software && _request.Program == ProgramName.SAP)
                    {
                        column.Item().Text(text => { text.Span("ประเภทปัญหา SAP: ").SemiBold(); text.Span(_request.SAPProblem.Value.ToString()); });
                    }

                    if (!string.IsNullOrEmpty(_request.AttachmentPath))
                    {
                         column.Item().Text(text => { text.Span("ไฟล์แนบ: ").SemiBold(); text.Span(_request.AttachmentPath); });
                    }
                });
            });
        }

        void ComposeApprovalSignatures(IContainer container)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Item().PaddingBottom(5).Text("ลายมือชื่อผู้เกี่ยวข้อง").Bold().FontSize(14);

                col.Item().Table(table =>
                {
                    var participants = new List<(string Title, string? Name, string? Role, DateTime? Date)>();

                    // 1. Requester
                    participants.Add(("ผู้ร้องขอ", _request.RequesterName, "ผู้ร้องขอ", _request.CreatedAt));

                    // 2. Approvers from History
                    var approvalHistories = _request.ApprovalHistories.OrderBy(h => h.ApprovalDate);
                    foreach (var history in approvalHistories)
                    {
                        var approver = _approvers.FirstOrDefault(u => u.FullName == history.ApproverName);
                        if (approver != null)
                        {
                            participants.Add((approver.Role ?? "DM", approver.FullName, approver.Role, history.ApprovalDate));
                        }
                    }
                    
                    // 3. IT Responsible User (If assigned)
                    if (_request.ResponsibleUser != null)
                    {
                        participants.Add(("ผู้รับผิดชอบ (IT)", _request.ResponsibleUser.FullName, "IT Support", _request.UpdatedAt));
                    }

                    const int maxColumns = 4;
                    table.ColumnsDefinition(columnsDef =>
                    {
                        for (int i = 0; i < maxColumns; i++)
                            columnsDef.RelativeColumn();
                    });

                    foreach (var p in participants)
                    {
                        table.Cell().Element(c => SignatureBlock(c, p.Title, p.Name, p.Role, p.Date));
                    }

                    // Fill remaining cells in the last row
                    var remainingCells = maxColumns - (participants.Count % maxColumns);
                    if (remainingCells > 0 && remainingCells < maxColumns)
                    {
                        for (int i = 0; i < remainingCells; i++)
                        {
                            table.Cell().Border(1, Colors.Grey.Lighten2); // Render an empty, bordered cell
                        }
                    }
                });
            });
        }


        void SignatureBlock(IContainer container, string title, string? name, string? role, DateTime? date)
        {
            container
                .Border(1, Colors.Grey.Lighten1)
                .Padding(5)
                .Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Text(title).SemiBold().AlignCenter();
                    column.Item().Height(40); // Signature space
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().Text(name ?? "-").AlignCenter().Bold();
                    column.Item().Text(role ?? "-").AlignCenter();
                    column.Item().Text(date?.ToLocalTime().ToString("dd/MM/yyyy") ?? "").AlignCenter();
                });
        }

        void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5).AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }
    }
}
