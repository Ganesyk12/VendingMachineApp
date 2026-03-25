using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VendingMachineApp.Models.ViewModels
{
    public class UserRoleListViewModel
    {
        public int IdUserRole { get; set; }
        public int IdUser { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserRoleCreateViewModel
    {
        [Required]
        [Display(Name = "Pilih User")]
        public int IdUser { get; set; }

        [Required]
        [Display(Name = "Nama Role")]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        public List<SelectListItem>? UserList { get; set; }
    }

    public class UserRoleEditViewModel
    {
        public int IdUserRole { get; set; }

        [Required]
        [Display(Name = "Pilih User")]
        public int IdUser { get; set; }

        [Required]
        [Display(Name = "Nama Role")]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        public List<SelectListItem>? UserList { get; set; }
    }
}
