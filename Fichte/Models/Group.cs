using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fichte.Models
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IDGroup { get; set; }
        
        [Required]
        public required string Name { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public required int CreatedByUserID { get; set; }
        
        public int MaxMembers { get; set; } = 50;
        
        public bool IsPrivate { get; set; } = false;
        
        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}