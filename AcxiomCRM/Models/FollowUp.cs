using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models
{
    public class FollowUp
    {
        [Key]
        public int FollowUpId { get; set; }

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int? LeadId { get; set; }

        [ForeignKey("LeadId")]
        public Lead? Lead { get; set; }

        public int? OpportunityId { get; set; }

        [ForeignKey("OpportunityId")]
        public Opportunity? Opportunity { get; set; }

        [Required(ErrorMessage = "Follow-Up Date is required.")]
        [DataType(DataType.Date)]
        public DateTime FollowUpDate { get; set; }

        [Required(ErrorMessage = "Subject is required."), StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string FollowUpType { get; set; } = "Call";

        [Required, StringLength(20)]
        public string Status { get; set; } = "Planned";

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        public string AssignedTo { get; set; } = string.Empty;

        [ForeignKey("AssignedTo")]
        public ApplicationUser? AssignedUser { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
