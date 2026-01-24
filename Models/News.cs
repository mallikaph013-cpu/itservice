using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime PublishedDate { get; set; }

        public string Status { get; set; } = "Active"; // Active, Inactive

        public string? ImagePath { get; set; } // Path to the uploaded image

        [NotMapped]
        [Display(Name = "Image File")]
        public IFormFile? ImageFile { get; set; }
    }
}
