using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models
{
    public class Opportunity
    {
        [Key]
        public int OpportunityId { get; set; }

        [Required(ErrorMessage = "Opportunity Name is required."), StringLength(200)]
        public string OpportunityName { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int? LeadId { get; set; }

        [ForeignKey("LeadId")]
        public Lead? Lead { get; set; }

        [Required(ErrorMessage = "Amount is required."), Range(0.01, double.MaxValue, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(30)]
        public string Stage { get; set; } = "Qualification";

        [Required(ErrorMessage = "Probability is required."), Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; }

        [Required(ErrorMessage = "Expected Close Date is required.")]
        [DataType(DataType.Date)]
        public DateTime ExpectedCloseDate { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        public string? AssignedTo { get; set; }

        [ForeignKey("AssignedTo")]
        public ApplicationUser? AssignedUser { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [NotMapped]
        public decimal WeightedValue => Amount * Probability / 100;

        public ICollection<FollowUp>? FollowUps { get; set; }
        public ICollection<Activity>? Activities { get; set; }
    }
}
