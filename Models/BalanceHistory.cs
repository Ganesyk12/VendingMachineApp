using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachineApp.Models
{
    public class BalanceHistory : BaseEntity
    {
        [Key]
        public int IdBalanceHistory { get; set; }

        [Required]
        [ForeignKey("User")]
        public int IdUser { get; set; }

        public decimal DebitBalance { get; set; } = 0; // Addition
        public decimal CreditBalance { get; set; } = 0; // Deduction
        
        public string? TransactionType { get; set; } // e.g., "Topup", "Purchase", "Refund"
        public string? Description { get; set; }

        public UserLogin? User { get; set; }
    }
}
