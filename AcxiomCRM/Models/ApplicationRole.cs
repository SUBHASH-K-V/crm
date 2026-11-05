using Microsoft.AspNetCore.Identity;

namespace AcxiomCRM.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
