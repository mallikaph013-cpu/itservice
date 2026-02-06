using System.Collections.Generic;

namespace myapp.Models
{
    public class SummaryReportViewModel
    {
        public Dictionary<RequestType, int> SummaryByRequestType { get; set; } = new Dictionary<RequestType, int>();
        public Dictionary<SupportRequestStatus, int> SummaryByStatus { get; set; } = new Dictionary<SupportRequestStatus, int>();
    }
}
