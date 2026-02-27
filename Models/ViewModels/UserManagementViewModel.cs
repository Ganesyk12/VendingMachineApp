using System.ComponentModel.DataAnnotations;

namespace VendingMachineApp.Models.ViewModels
{
    public class UserListViewModel
    {
        public int IdUser { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nama Lengkap")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Konfirmasi Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserEditViewModel
    {
        public int IdUser { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nama Lengkap")]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Password Baru (Biarkan kosong jika tidak ingin mengubah)")]
        public string? Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Konfirmasi Password Baru")]
        public string? ConfirmPassword { get; set; }
    }
}
