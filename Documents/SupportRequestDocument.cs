using myapp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace myapp.Documents
{
    public class SupportRequestDocument : IDocument
    {
        private readonly SupportRequest _request;
        private readonly List<Approver> _approvalSteps;
        private readonly TimeZoneInfo _thaiZone = TimeZoneInfo.CreateCustomTimeZone("SE Asia Standard Time", TimeSpan.FromHours(7), "(UTC+07:00) Bangkok, Hanoi, Jakarta", "SE Asia Standard Time");

        public SupportRequestDocument(SupportRequest request, List<Approver> approvalSteps)
        {
            _request = request;
            _approvalSteps = approvalSteps;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Sarabun").FontSize(12));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column => 
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(innerColumn =>
                    {
                        innerColumn.Item().Text("IT Service Request").Bold().FontSize(22);
                        innerColumn.Item().Text($"No: {_request.DocumentNo ?? _request.Id.ToString("D6")}").SemiBold();
                        innerColumn.Item().Text($"Date: {TimeZoneInfo.ConvertTimeFromUtc(_request.CreatedAt, _thaiZone):dd/MM/yyyy HH:mm}");
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingTop(15).Column(column =>
            {
                column.Spacing(20);
                column.Item().Element(ComposeRequesterDetails);
                column.Item().Element(ComposeRequestInfo);
                column.Item().Element(ComposeDescription);
                column.Item().Element(ComposeApprovalHistory);
            });
        }

        void ComposeRequesterDetails(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("Requester Details").Bold().FontSize(14);
                column.Item().Text(text => { text.Span("Name: ").SemiBold(); text.Span(_request.RequesterName ?? "-"); });
                column.Item().Text(text => { text.Span("Department: ").SemiBold(); text.Span(_request.Department ?? "-"); });
                column.Item().Text(text => { text.Span("Employee ID: ").SemiBold(); text.Span(_request.EmployeeId ?? "-"); });
            });
        }

        void ComposeRequestInfo(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Item().Text("Request Information").Bold().FontSize(14);
                column.Item().PaddingTop(5).Grid(grid =>
                {
                    grid.Columns(2);
                    grid.Item().Text(text => { text.Span("Request Type: ").SemiBold(); text.Span(_request.RequestType.ToString()); });
                    grid.Item().Text(text => { text.Span("Urgency: ").SemiBold(); text.Span(_request.Urgency.ToString()); });
                    grid.Item().Text(text => { text.Span("Status: ").SemiBold(); text.Span(_request.Status.ToString()); });
                    grid.Item().Text(text => { text.Span("Handler: ").SemiBold(); text.Span(_request.ResponsibleUser?.FullName ?? "Unassigned"); });
                });
            });
        }

        void ComposeDescription(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Item().Text("Details").Bold().FontSize(14);
                column.Item().PaddingTop(5).Column(col =>
                {
                    if (_request.Program == ProgramName.SAP)
                    {
                        col.Item().Element(ComposeSAPDetails);
                    }
                    else
                    {
                        col.Item().Text(_request.ProblemDescription ?? "No description provided.");
                    }
                });
            });
        }

        void ComposeSAPDetails(IContainer container)
        {
            container.Grid(grid =>
            {
                grid.VerticalSpacing(5);
                grid.Columns(12);

                void AddDetail(string label, string? value, int labelCols = 3, int valueCols = 9)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        grid.Item(labelCols).Text(label).SemiBold();
                        grid.Item(valueCols).Text(value);
                    }
                }

                AddDetail("Problem Type:", _request.SAPProblem?.ToString(), 4, 8);
                
                grid.Item(12).PaddingTop(5).Text("Material Types:").Bold();
                grid.Item(3).Text($"FG: {(_request.IsFG ? "Yes" : "No")}");
                grid.Item(3).Text($"SM: {(_request.IsSM ? "Yes" : "No")}");
                grid.Item(3).Text($"RM: {(_request.IsRM ? "Yes" : "No")}");
                grid.Item(3).Text($"Tooling: {(_request.IsTooling ? "Yes" : "No")}");

                grid.Item(12).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

                AddDetail("ICS Code:", _request.ICSCode, 4, 8);
                AddDetail("English Desc:", _request.EnglishMatDescription, 4, 8);
                AddDetail("Material Group:", _request.MaterialGroup, 4, 8);
                AddDetail("Division:", _request.Division, 4, 8);
                AddDetail("Profit Center:", _request.ProfitCenter, 4, 8);
                AddDetail("Distr. Channel:", _request.DistributionChannel, 4, 8);

                if (!string.IsNullOrWhiteSpace(_request.ProblemDescription))
                {
                    grid.Item(12).PaddingTop(10);
                    AddDetail("Additional Details:", _request.ProblemDescription, 12, 12);
                }
            });
        }

        void ComposeApprovalHistory(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Approval Signatures").Bold().FontSize(14);
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.Spacing(15);

                    // --- 1. Requester ---
                    row.RelativeItem().Column(col =>
                    {
                        col.Spacing(2);
                        col.Item().AlignCenter().Text(_request.RequesterName ?? "N/A").Bold();
                        col.Item().AlignCenter().Text("ผู้ร้องขอ");
                        col.Item().PaddingTop(5).AlignCenter().Text(TimeZoneInfo.ConvertTimeFromUtc(_request.CreatedAt, _thaiZone).ToString("dd/MM/yy"));
                    });

                    // --- 2. Approvers ---
                    var orderedSteps = _approvalSteps.OrderBy(a => a.Order);

                    foreach (var step in orderedSteps)
                    {
                        var history = _request.ApprovalHistories?.FirstOrDefault(h => h.ApproverName == step.User.EmployeeId);

                        row.RelativeItem().Column(col =>
                        {
                            col.Spacing(2);
                            col.Item().AlignCenter().Text(step.User.FullName ?? "(Pending)").Bold();
                            col.Item().AlignCenter().Text(step.User.Role ?? "N/A");
                            
                            var approvalDate = (history != null && history.Approved)
                                ? TimeZoneInfo.ConvertTimeFromUtc(history.ApprovalDate, _thaiZone).ToString("dd/MM/yy")
                                : "-";

                            col.Item().PaddingTop(5).AlignCenter().Text(approvalDate);
                        });
                    }
                });
            });
        }
        
        public byte[] GeneratePdf()
        {
            return Document.Create(container => Compose(container)).GeneratePdf();
        }
    }
}
