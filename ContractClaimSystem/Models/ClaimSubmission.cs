using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractClaimSystem.Models
{
    public class ClaimSubmission
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to ApplicationUser

        [ForeignKey("UserId")]
        public ApplicationUser Lecturer { get; set; } // Navigation property

        [Required(ErrorMessage = "Hours worked is required.")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required.")]
        public decimal HourlyRate { get; set; }

        public string? AdditionalNotes { get; set; }

        public DateTime ClaimDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        public string? SupportingDocument { get; set; } // Made non-required

        [NotMapped]
        public decimal FinalPayment => HoursWorked * HourlyRate;
    }
}
