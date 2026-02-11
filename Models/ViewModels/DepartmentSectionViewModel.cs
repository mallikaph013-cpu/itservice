using System.Collections.Generic;
using myapp.Models;

namespace myapp.Models.ViewModels
{
    public class DepartmentSectionViewModel
    {
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
        public IEnumerable<Section> Sections { get; set; } = new List<Section>();
    }
}
