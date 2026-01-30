using myapp.Models;
using System.Collections.Generic;

namespace myapp.ViewModels
{
    public class ApprovalIndexViewModel
    {
        public IEnumerable<SupportRequest> PendingRequests { get; set; } = new List<SupportRequest>();
        public IEnumerable<User> AssignableUsers { get; set; } = new List<User>();
    }
}
