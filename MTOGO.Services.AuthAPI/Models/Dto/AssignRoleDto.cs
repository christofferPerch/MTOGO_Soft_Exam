using System.ComponentModel.DataAnnotations;

namespace MTOGO.Services.AuthAPI.Models.Dto
{
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public required string Role { get; set; }
    }
}
