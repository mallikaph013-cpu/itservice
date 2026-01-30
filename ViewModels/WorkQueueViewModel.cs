using myapp.Models;
using System.Collections.Generic;

namespace myapp.ViewModels
{
    public class WorkQueueViewModel
    {
        public IEnumerable<SupportRequest>? SupportRequests { get; set; }
        public IEnumerable<User>? AssignableUsers { get; set; }
    }
}
