using System;
using System.ComponentModel.DataAnnotations;


namespace VendingMachineApp.Models;

public class Product
{
    [Key]
    public int IdProduct { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
