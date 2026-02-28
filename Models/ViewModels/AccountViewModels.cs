using System.ComponentModel.DataAnnotations;

namespace VendingMachineApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kode verifikasi harus diisi.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kode verifikasi berupa 6 digit angka.")]
        public string VerificationCode { get; set; } = string.Empty;
    }
}
