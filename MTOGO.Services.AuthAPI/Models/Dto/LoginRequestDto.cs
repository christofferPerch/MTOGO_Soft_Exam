using System.ComponentModel.DataAnnotations;

namespace MTOGO.Services.AuthAPI.Models.Dto
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
