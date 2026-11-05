using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class OpportunityViewModel
    {
        public int OpportunityId { get; set; }

        [Required(ErrorMessage = "Opportunity Name is required.")]
        [StringLength(200)]
        public string OpportunityName { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        public int? LeadId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public string Stage { get; set; } = "Qualification";

        [Required(ErrorMessage = "Probability is required.")]
        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; }

        [Required(ErrorMessage = "Expected Close Date is required.")]
        [DataType(DataType.Date)]
        public DateTime ExpectedCloseDate { get; set; }

        [Required]
        public string Status { get; set; } = "Open";

        public string? AssignedTo { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

