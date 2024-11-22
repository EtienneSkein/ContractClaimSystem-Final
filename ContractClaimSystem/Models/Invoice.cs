using System;
using System.Collections.Generic;

//Currently this class is only being used locally and not in the dbcontext, fix in future
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
