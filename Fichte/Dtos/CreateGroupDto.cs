using System.ComponentModel.DataAnnotations;

namespace Fichte.Dtos
{
    public class CreateGroupDto
    {
        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters.")]
        public required string Name { get; set; }
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        
        [Range(2, 50, ErrorMessage = "Max members must be between 2 and 50.")]
        public int MaxMembers { get; set; } = 50;
        
    }
}