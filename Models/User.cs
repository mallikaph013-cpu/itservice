
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string EmployeeId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public required string Password { get; set; }
        public required string Department { get; set; }
        public required string Role { get; set; }
    }
}
