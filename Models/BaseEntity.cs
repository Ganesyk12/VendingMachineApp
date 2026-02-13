using System;

namespace VendingMachineApp.Models
{
    public abstract class BaseEntity
    {
        public string UserCreated { get; set; } = "SYSTEM";
        public string UserModified { get; set; } = "SYSTEM";
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public string Status { get; set; } = "A"; // Default 'A' for Active
    }
}
