using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachineApp.Models
{
    public class UserBalance : BaseEntity
    {
        [Key] public int IdUserBalance { get; set; }

        [Required] [ForeignKey("User")] public int IdUser { get; set; }

        public decimal Balance { get; set; } = 0;

        [Required] public string Name { get; set; } = string.Empty;

        public UserLogin? User { get; set; }
    }
}
