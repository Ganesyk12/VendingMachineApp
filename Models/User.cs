using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VendingMachineApp.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string? Name { get; set; }
    public decimal? Balance { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
