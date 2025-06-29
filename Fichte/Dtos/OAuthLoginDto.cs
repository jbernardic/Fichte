using System.ComponentModel.DataAnnotations;

namespace Fichte.Dtos
{
    public class OAuthLoginDto
    {
        [Required]
        public required string Provider { get; set; } // "google" or "github"
        
        [Required]
        public required string Code { get; set; }
        
        public string? State { get; set; }
    }
}