using System.ComponentModel.DataAnnotations;

namespace VendingMachineApp.Models.ViewModels
{
   public class SmtpSettingViewModel
   {
      public int IdSmtp { get; set; }

      [Required(ErrorMessage = "Host SMTP tidak boleh kosong")]
      [StringLength(100)]
      [Display(Name = "SMTP Host")]
      public string Host { get; set; } = string.Empty;

      [Required(ErrorMessage = "Port tidak boleh kosong")]
      [Display(Name = "SMTP Port")]
      public int Port { get; set; } = 587;

      [Display(Name = "Gunakan SSL/TLS")] public bool EnableSSL { get; set; } = true;

      [Required(ErrorMessage = "Nama Pengirim tidak boleh kosong")]
      [StringLength(100)]
      [Display(Name = "Nama Pengirim (Sender Name)")]
      public string SenderName { get; set; } = string.Empty;

      [Required(ErrorMessage = "Email Pengirim tidak boleh kosong")]
      [EmailAddress(ErrorMessage = "Format email tidak valid")]
      [StringLength(100)]
      [Display(Name = "Email Pengirim (Sender Email)")]
      public string SenderEmail { get; set; } = string.Empty;

      [Required(ErrorMessage = "Username tidak boleh kosong")]
      [StringLength(100)]
      [Display(Name = "Username Autentikasi")]
      public string Username { get; set; } = string.Empty;

      [Required(ErrorMessage = "Password tidak boleh kosong")]
      [DataType(DataType.Password)]
      [StringLength(255)]
      [Display(Name = "Password Autentikasi / App Password")]
      public string Password { get; set; } = string.Empty;

      [Display(Name = "Jadikan Konfigurasi Aktif")]
      public bool IsActive { get; set; } = true;
   }
}
