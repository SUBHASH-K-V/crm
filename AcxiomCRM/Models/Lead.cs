using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models
{
    public class Lead
    {
        [Key]
        public int LeadId { get; set; }

        [Required, StringLength(20)]
        public string LeadCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lead Name is required."), StringLength(150)]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required."), Phone(ErrorMessage = "Enter a valid phone number."), StringLength(15), RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [Required, StringLength(50)]
        public string Source { get; set; } = "Website";

        [Required(ErrorMessage = "Lead Status is required."), StringLength(20)]
        public string Status { get; set; } = "New";

        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        [Range(0, 100000000, ErrorMessage = "Expected value must be between 0 and 100,000,000.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? AssignedTo { get; set; }

        [ForeignKey("AssignedTo")]
        public ApplicationUser? AssignedUser { get; set; }

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public ICollection<FollowUp>? FollowUps { get; set; }
        public ICollection<Activity>? Activities { get; set; }
    }
}
