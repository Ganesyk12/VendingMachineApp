using System;
using System.ComponentModel.DataAnnotations;


namespace VendingMachineApp.Models;

public class Product : BaseEntity
{
    [Key] public int IdProduct { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
}
