using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VendingMachineApp.Models
{
    public class UserLogin : BaseEntity
    {
        [Key] public int IdUser { get; set; }

        [Required] [EmailAddress] public string UserName { get; set; }

        [Required] public string Password { get; set; }


        public UserBalance? UserBalance { get; set; }
        public ICollection<BalanceHistory> BalanceHistories { get; set; } = new List<BalanceHistory>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
