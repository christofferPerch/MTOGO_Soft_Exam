using System.ComponentModel.DataAnnotations;

namespace MTOGO.Web.Models.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
