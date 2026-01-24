using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class BomComponent
    {
        public int Id { get; set; }

        [Display(Name = "Item")]
        public int? Item { get; set; }

        [Display(Name = "Item Category")]
        public string? ItemCategory { get; set; } = "L";

        [Display(Name = "Component Number")]
        public string? ComponentNumber { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Item Quantity")]
        public int? ItemQuantity { get; set; }

        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        [Display(Name = "BOM Unit")]
        public string? BomUnit { get; set; }

        [Display(Name = "Sloc")]
        public string? Sloc { get; set; }

        // Foreign key to SupportRequest
        public int SupportRequestId { get; set; }
        public SupportRequest? SupportRequest { get; set; }
    }
}
