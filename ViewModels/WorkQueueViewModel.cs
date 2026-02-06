using Microsoft.AspNetCore.Mvc.Rendering;
using myapp.Models;
using System.Collections.Generic;

namespace myapp.ViewModels
{
    public class WorkQueueViewModel
    {
        public IEnumerable<SupportRequest>? SupportRequests { get; set; }
        public IEnumerable<User>? AssignableUsers { get; set; }
        public SelectList? StatusOptions { get; set; }
        public SupportRequestStatus? SelectedStatus { get; set; }
    }
}
