using myapp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace myapp.ViewModels
{
    public class ApprovalIndexViewModel
    {
        // A single list to hold the requests based on the selected filter
        public List<SupportRequest> Requests { get; set; } = new List<SupportRequest>();

        // Properties for the filter dropdown
        public SupportRequestStatus? SelectedStatus { get; set; }
        public SelectList? StatusOptions { get; set; }

        // The following properties are now obsolete as we use a single list, 
        // but kept for reference or future complex views.
        // public List<SupportRequest> PendingRequests { get; set; } = new List<SupportRequest>();
        // public List<SupportRequest> ApprovedRequests { get; set; } = new List<SupportRequest>();
        // public List<SupportRequest> RejectedRequests { get; set; } = new List<SupportRequest>();
        public List<User> AssignableUsers { get; set; } = new List<User>();

    }
}
