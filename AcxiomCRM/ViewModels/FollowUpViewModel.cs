using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class FollowUpViewModel
    {
        public int FollowUpId { get; set; }

        public int? CustomerId { get; set; }
        public int? LeadId { get; set; }
        public int? OpportunityId { get; set; }

        [Required(ErrorMessage = "Follow-Up Date is required.")]
        [DataType(DataType.Date)]
        public DateTime FollowUpDate { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string FollowUpType { get; set; } = "Call";

        [Required]
        public string Status { get; set; } = "Planned";

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        public string AssignedTo { get; set; } = string.Empty;
    }
}

