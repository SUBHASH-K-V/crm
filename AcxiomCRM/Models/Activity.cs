using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required, StringLength(30)]
        public string ActivityType { get; set; } = "Call";

        [Required(ErrorMessage = "Subject is required."), StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int? LeadId { get; set; }

        [ForeignKey("LeadId")]
        public Lead? Lead { get; set; }

        public int? OpportunityId { get; set; }

        [ForeignKey("OpportunityId")]
        public Opportunity? Opportunity { get; set; }

        [Required]
        public string AssignedTo { get; set; } = string.Empty;

        [ForeignKey("AssignedTo")]
        public ApplicationUser? AssignedUser { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Planned";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
