using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class SupportRequest
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "รหัสพนักงาน")]
        public string? EmployeeId { get; set; }

        [Required]
        [Display(Name = "ชื่อผู้แจ้ง")]
        public string? RequesterName { get; set; }

        [Required]
        [Display(Name = "ฝ่าย")]
        public string? Department { get; set; }

        [Required]
        [Display(Name = "ประเภทการแจ้งซ่อม")]
        public RequestType RequestType { get; set; }

        [Display(Name = "โปรแกรม")]
        public ProgramName? Program { get; set; }

        [Display(Name = "ประเภทปัญหา SAP")]
        public SAPProblemType? SAPProblem { get; set; }

        // SAP Registration Fields
        [Display(Name = "FG")]
        public bool IsFG { get; set; }
        [Display(Name = "SM")]
        public bool IsSM { get; set; }
        [Display(Name = "RM")]
        public bool IsRM { get; set; }
        [Display(Name = "Tooling")]
        public bool IsTooling { get; set; }
        [Display(Name = "ICS Code")]
        public string? ICSCode { get; set; }
        [Display(Name = "English Mat/Description")]
        public string? EnglishMatDescription { get; set; }
        [Display(Name = "Material Group")]
        public string? MaterialGroup { get; set; }
        [Display(Name = "Division")]
        public string? Division { get; set; }
        [Display(Name = "Profit Center")]
        public string? ProfitCenter { get; set; }
        [Display(Name = "Distribution Channel")]
        public string? DistributionChannel { get; set; }
        [Display(Name = "BOI Code")]
        public string? BOICode { get; set; }
        [Display(Name = "MRP Controller")]
        public string? MRPController { get; set; }
        [Display(Name = "Storage Loc")]
        public string? StorageLoc { get; set; }
        [Display(Name = "Storage Loc(BP**)")]
        public string? StorageLocBP { get; set; }
        [Display(Name = "Storage Loc(B1**)")]
        public string? StorageLocB1 { get; set; }
        [Display(Name = "Production Supervisor")]
        public string? ProductionSupervisor { get; set; }
        [Display(Name = "Costing Lot Size")]
        public string? CostingLotSize { get; set; }
        [Display(Name = "Valuation Class")]
        public string? ValClass { get; set; }
        [Display(Name = "Plant")]
        public string? Plant { get; set; }
        [Display(Name = "Base Unit")]
        public string? BaseUnit { get; set; }
        [Display(Name = "Maker / Mfr Part Number")]
        public string? MakerMfrPartNumber { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Price")]
        public decimal? Price { get; set; }
        [Display(Name = "Per Price")]
        public int? PerPrice { get; set; }

        // New Fields from previous request
        [Display(Name = "BOI Description")]
        public string? BOIDescription { get; set; }
        [Display(Name = "Purcasing Group")]
        public string? PurchasingGroup { get; set; }
        [Display(Name = "Comm Code/ Tariff Code")]
        public string? CommCodeTariffCode { get; set; }
        [Display(Name = "% traiff code ทาง Purchasing")]
        public string? TariffCodePercentage { get; set; }
        [Display(Name = "Price Control (S/V)")]
        public string? PriceControl { get; set; }
        [Display(Name = "Supplier Code")]
        public string? SupplierCode { get; set; }

        // New Field for this request
        [Display(Name = "Model Name")]
        public string? ModelName { get; set; }

        // BOM Components
        public List<BomComponent> BomComponents { get; set; } = new List<BomComponent>();

        [Required]
        [Display(Name = "รายละเอียดปัญหา")]
        public string? ProblemDescription { get; set; }

        [NotMapped]
        [Display(Name = "ไฟล์แนบ")]
        public IFormFile? Attachment { get; set; }

        public string? AttachmentPath { get; set; }

        [Required]
        [Display(Name = "ความเร่งด่วน")]
        public UrgencyLevel Urgency { get; set; }

        [Display(Name = "สถานะ")]
        public SupportRequestStatus Status { get; set; } = SupportRequestStatus.Pending;

        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "วันที่แก้ไข")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "ผู้สร้าง")]
        public string? CreatedBy { get; set; }

        [Display(Name = "ผู้แก้ไข")]
        public string? UpdatedBy { get; set; }

        public List<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();

        [Display(Name = "ผู้อนุมัติปัจจุบัน")]
        public int? CurrentApproverId { get; set; }

        [ForeignKey("CurrentApproverId")]
        public Approver? CurrentApprover { get; set; }
    }
}
