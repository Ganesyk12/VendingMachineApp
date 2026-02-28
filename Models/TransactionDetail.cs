using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachineApp.Models;

public class TransactionDetail : BaseEntity
{
    [Key]
    public int IdDetail { get; set; }

    [ForeignKey("UserTransaction")]
    public int IdTransaction { get; set; }

    [ForeignKey("Product")]
    public int? IdProduct { get; set; }

    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }

    public UserTransaction? UserTransaction { get; set; }
    public Product? Product { get; set; }
}
