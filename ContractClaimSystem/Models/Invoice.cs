using System;
using System.Collections.Generic;

namespace ContractClaimSystem.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public string LecturerName { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Generated"; // Default status is "Generated"

        // A list of claims associated with this invoice
        public List<ClaimSubmission> Claims { get; set; } = new List<ClaimSubmission>();
    }
}
