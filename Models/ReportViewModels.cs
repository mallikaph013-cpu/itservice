using System.Collections.Generic;

namespace myapp.Models
{
    public class ReportViewModel
    {
        public List<ReportItemViewModel> ReportItems { get; set; } = new List<ReportItemViewModel>();
    }

    public class ReportItemViewModel
    {
        public SupportRequest Request { get; set; } = new SupportRequest();
        public string? ResolutionTime { get; set; }
    }
}
