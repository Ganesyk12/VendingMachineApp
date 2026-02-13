using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachineApp.Models;

public class Transaction : BaseEntity
{
    [Key] public int TransactionId { get; set; }

    [ForeignKey("User")] public int IdUser { get; set; }

    [ForeignKey("Product")] public int? IdProduct { get; set; }

    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public decimal? BalanceAfterTransaction { get; set; }
    public string? TransactionType { get; set; }
    public string? TrxCode { get; set; }

    public UserLogin? User { get; set; }
    public Product? Product { get; set; }
}
