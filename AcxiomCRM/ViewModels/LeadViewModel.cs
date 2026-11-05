using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class LeadViewModel
    {
        public int LeadId { get; set; }

        [Required(ErrorMessage = "Lead Name is required.")]
        [StringLength(150)]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [Required]
        public string Source { get; set; } = "Website";

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "New";

        public string Priority { get; set; } = "Medium";

        [Range(0, 100000000, ErrorMessage = "Expected value must be between 0 and 100,000,000.")]
        public decimal ExpectedValue { get; set; }

        public string? AssignedTo { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

