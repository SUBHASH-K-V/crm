using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class ActivityViewModel
    {
        public int ActivityId { get; set; }

        [Required]
        public string ActivityType { get; set; } = "Call";

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ActivityDate { get; set; } = DateTime.Today;

        public int? CustomerId { get; set; }
        public int? LeadId { get; set; }

        [Required]
        public string AssignedTo { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Planned";
    }
}

