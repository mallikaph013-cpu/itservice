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
        private readonly TimeZoneInfo _thaiZone = TimeZoneInfo.CreateCustomTimeZone("SE Asia Standard Time", TimeSpan.FromHours(7), "(UTC+07:00) Bangkok, Hanoi, Jakarta", "SE Asia Standard Time");

        public SupportRequestDocument(SupportRequest request)
        {
            _request = request;
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
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Service Required").Bold().FontSize(36);
                });

                column.Item().Row(row =>
                {
                    var docNo = _request.DocumentNo ?? $"SR-{_request.Id:D6}";
                    row.RelativeItem().Text(docNo).SemiBold().FontSize(16);
                });

                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Black);
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingTop(15).Column(column =>
            {
                column.Item().Element(ComposeRequesterDetails);
                column.Item().PaddingTop(10).Element(ComposeRequestInfo);
                column.Item().PaddingTop(10).Element(ComposeDescription);
                column.Item().PaddingTop(20).Element(ComposeApprovalHistory);
            });
        }

        void ComposeRequesterDetails(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("รายละเอียดผู้ร้องขอ").Bold().FontSize(16);
                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("ชื่อ: ").SemiBold();
                    text.Span(_request.RequesterName ?? "-");
                });
                column.Item().Text(text =>
                {
                    text.Span("แผนก: ").SemiBold();
                    text.Span(_request.Department ?? "-");
                });
                 column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Black);
            });
        }

        void ComposeRequestInfo(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("รายละเอียดการแจ้งซ่อม").Bold().FontSize(16);
                column.Item().PaddingTop(5).Grid(grid =>
                {
                    grid.Columns(2);
                    grid.Item().Text(text => { text.Span("ประเภท: ").SemiBold(); text.Span(_request.RequestType.ToString()); });
                    grid.Item().Text(text => { text.Span("ความเร่งด่วน: ").SemiBold(); text.Span(_request.Urgency.ToString()); });
                    grid.Item().Text(text => { text.Span("สถานะปัจจุบัน: ").SemiBold(); text.Span(_request.Status.ToString()); });
                    grid.Item().Text(text => { text.Span("ผู้รับผิดชอบ: ").SemiBold(); text.Span(_request.ResponsibleUser?.FullName ?? "-"); });
                });
                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Black);
            });
        }

        void ComposeDescription(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("รายละเอียด").Bold().FontSize(16);
                
                // Conditional content based on Program Name
                if (_request.Program == ProgramName.SAP)
                {
                    column.Item().PaddingTop(5).Element(ComposeSAPDetails);
                }
                else
                {
                    column.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).MinHeight(100).Column(col =>
                    {
                        col.Item().Text(_request.ProblemDescription ?? "ไม่มีคำอธิบาย");
                    });
                }

                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Black);
            });
        }

        void ComposeSAPDetails(IContainer container)
        {
            container.Grid(grid =>
            {
                grid.Columns(12); // Use a 12-column grid for flexibility

                // Helper to add a detail row
                void AddDetail(string label, string? value, int labelCols = 3, int valueCols = 9)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        grid.Item(labelCols).Text(label).SemiBold();
                        grid.Item(valueCols).Text(value);
                    }
                }
                
                // Helper to add a boolean detail row
                void AddBooleanDetail(string label, bool value, int labelCols = 3, int valueCols = 9)
                {
                    grid.Item(labelCols).Text(label).SemiBold();
                    grid.Item(valueCols).Text(value ? "Yes" : "No");
                }

                AddDetail("Program", _request.Program?.ToString());
                AddDetail("SAP Problem Type", _request.SAPProblem?.ToString());

                // Material Types
                grid.Item(12).PaddingTop(5).Text("Material Types:").Bold();
                grid.Item(3).Text("FG: " + (_request.IsFG ? "Yes" : "No"));
                grid.Item(3).Text("SM: " + (_request.IsSM ? "Yes" : "No"));
                grid.Item(3).Text("RM: " + (_request.IsRM ? "Yes" : "No"));
                grid.Item(3).Text("Tooling: " + (_request.IsTooling ? "Yes" : "No"));
                
                grid.Item(12).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                grid.Item(12).PaddingTop(5); // Spacer

                // Registration Fields
                AddDetail("ICS Code", _request.ICSCode, 4, 8);
                AddDetail("English Mat/Desc", _request.EnglishMatDescription, 4, 8);
                AddDetail("Material Group", _request.MaterialGroup, 4, 8);
                AddDetail("Division", _request.Division, 4, 8);
                AddDetail("Profit Center", _request.ProfitCenter, 4, 8);
                AddDetail("Distribution Channel", _request.DistributionChannel, 4, 8);
                AddDetail("BOI Code", _request.BOICode, 4, 8);
                AddDetail("MRP Controller", _request.MRPController, 4, 8);
                AddDetail("Storage Loc", _request.StorageLoc, 4, 8);
                AddDetail("Storage Loc(BP**)", _request.StorageLocBP, 4, 8);
                AddDetail("Storage Loc(B1**)", _request.StorageLocB1, 4, 8);
                AddDetail("Production Supervisor", _request.ProductionSupervisor, 4, 8);
                AddDetail("Costing Lot Size", _request.CostingLotSize, 4, 8);
                AddDetail("Valuation Class", _request.ValClass, 4, 8);
                AddDetail("Plant", _request.Plant, 4, 8);
                AddDetail("Base Unit", _request.BaseUnit, 4, 8);
                AddDetail("Maker / Mfr Part", _request.MakerMfrPartNumber, 4, 8);
                AddDetail("Price", _request.Price?.ToString("N2"), 4, 8);
                AddDetail("Per Price", _request.PerPrice?.ToString(), 4, 8);
                AddDetail("BOI Description", _request.BOIDescription, 4, 8);
                AddDetail("Purchasing Group", _request.PurchasingGroup, 4, 8);
                AddDetail("Comm Code/Tariff", _request.CommCodeTariffCode, 4, 8);
                AddDetail("% Tariff (Purchasing)", _request.TariffCodePercentage, 4, 8);
                AddDetail("Price Control (S/V)", _request.PriceControl, 4, 8);
                AddDetail("Supplier Code", _request.SupplierCode, 4, 8);
                AddDetail("Model Name", _request.ModelName, 4, 8);

                // General Problem Description at the end
                grid.Item(12).PaddingTop(10); // Spacer
                AddDetail("More Details", _request.ProblemDescription, 12, 12);
            });
        }


        void ComposeApprovalHistory(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("การอนุมัติ").Bold().FontSize(16);
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.Spacing(15);

                    // Header
                    row.RelativeItem(2).Text("ตำแหน่ง").Bold();
                    row.RelativeItem(3).Text("ผู้อนุมัติ").Bold();
                    row.RelativeItem(2).Text("สถานะ").Bold();
                    row.RelativeItem(2).Text("วันที่").Bold();
                });

                // Divider
                column.Item().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

                // Requester row
                 column.Item().Row(row =>
                 {
                     row.Spacing(15);
                     row.RelativeItem(2).Text("ผู้ร้องขอ");
                     row.RelativeItem(3).Text(_request.RequesterName ?? "-");
                     row.RelativeItem(2).Text("Submitted").FontColor(Colors.Grey.Darken1);
                     row.RelativeItem(2).Text(TimeZoneInfo.ConvertTimeFromUtc(_request.CreatedAt, _thaiZone).ToString("dd/MM/yy HH:mm"));
                 });


                // History rows
                if (_request.ApprovalHistories != null)
                {
                    foreach (var history in _request.ApprovalHistories.OrderBy(h => h.ApprovalDate))
                    {
                         column.Item().PaddingTop(5).Row(row =>
                         {
                            row.Spacing(15);
                            row.RelativeItem(2).Text("-"); // Placeholder for Role
                            row.RelativeItem(3).Text(history.ApproverName ?? "-");
                            row.RelativeItem(2).Text(history.Approved ? "Approved" : "Rejected").FontColor(history.Approved ? Colors.Green.Medium : Colors.Red.Medium);
                            row.RelativeItem(2).Text(TimeZoneInfo.ConvertTimeFromUtc(history.ApprovalDate, _thaiZone).ToString("dd/MM/yy HH:mm"));
                         });
                    }
                }
            });
        }

        public byte[] GeneratePdf()
        {
            return Document.Create(container => Compose(container)).GeneratePdf();
        }
    }
}
