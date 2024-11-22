using System;
using System.Collections.Generic;

namespace ContractClaimSystem.Models
{
    //This works based on the identity custom class
    public class Lecturer
    {
        public int Id { get; set; }
        public string Name { get; set; } // Lecturer's Name
        public string Email { get; set; } // Email Address
        public string ContactNumber { get; set; } // Contact Number
        public string Department { get; set; } // Department Name

        // Navigation property for related claims
        public List<ClaimSubmission> Claims { get; set; } = new List<ClaimSubmission>();
    }
}
