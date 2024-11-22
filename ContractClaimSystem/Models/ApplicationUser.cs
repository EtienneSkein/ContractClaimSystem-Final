using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ContractClaimSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty; // Default value
        public string Department { get; set; } = string.Empty; // Default value

        // Navigation property for related claims
        public List<ClaimSubmission> Claims { get; set; } = new List<ClaimSubmission>();
    }
}
