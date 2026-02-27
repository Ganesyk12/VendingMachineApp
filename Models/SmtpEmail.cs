using System.ComponentModel.DataAnnotations;

namespace VendingMachineApp.Models
{
    public class SmptpEmail : BaseEntity
    {
        [Key]
        public int IdSmtp { get; set; }
        [Required, StringLength(100)]
        public string Host { get; set; } = string.Empty;
        [Required]
        public int Port { get; set; }
        public bool EnableSSL { get; set; } = true;
        [Required, StringLength(100)]
        public string SenderName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(100)]
        public string SenderEmail { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required, StringLength(255)]
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }
}
