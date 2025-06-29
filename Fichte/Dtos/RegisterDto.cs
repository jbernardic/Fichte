using System.ComponentModel.DataAnnotations;

namespace Fichte.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }
    }
}