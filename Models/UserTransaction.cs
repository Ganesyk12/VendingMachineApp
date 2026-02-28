using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachineApp.Models;

public class UserTransaction : BaseEntity
{
    [Key]
    public int IdTransaction { get; set; }

    [ForeignKey("User")]
    public int IdUser { get; set; }

    public string? TrxCode { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Date { get; set; }
    public decimal? BalanceAfterTransaction { get; set; }
    public string? TransactionType { get; set; }

    public UserLogin? User { get; set; }
    public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
}
