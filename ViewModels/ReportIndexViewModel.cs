using myapp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace myapp.ViewModels
{
    public class ReportIndexViewModel
    {
        public List<SupportRequest> Requests { get; set; } = new List<SupportRequest>();
        public SupportRequestStatus? SelectedStatus { get; set; }
        public SelectList? StatusOptions { get; set; }

        // For summary cards
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int DoneRequests { get; set; }
    }
}
