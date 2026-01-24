using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public enum RequestType
    {
        Hardware,
        Software,
        Network,
        Other
    }

    public enum ProgramName
    {
        SAP,
        Email,
        Office365,
        Other
    }

    public enum UrgencyLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum SAPProblemType
    {
        [Display(Name = "แจ้งปัญหาโปรแกรม/Report")]
        Issue,
        [Display(Name = "ขึ้นทะเบียน FG,SM,RM")]
        NewRegistration,
        [Display(Name = "สร้าง BOM")]
        CreateBOM
    }

    public enum SupportRequestStatus
    {
        [Display(Name = "รออนุมัติ")]
        Pending,
        [Display(Name = "อนุมัติแล้ว/รอรับงาน")]
        Approved,
        [Display(Name = "กำลังดำเนินการ")]
        InProgress,
        [Display(Name = "ปิดงาน")]
        Done,
        [Display(Name = "ไม่อนุมัติ")]
        Rejected
    }
}
