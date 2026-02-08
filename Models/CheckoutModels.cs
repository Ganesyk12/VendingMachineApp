using System.Collections.Generic;

namespace VendingMachineApp.Models;

public class CheckoutRequest
{
    public int IdUser { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
