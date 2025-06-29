using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fichte.Models
{
    public class GroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IDGroupMember { get; set; }
        
        public required int GroupID { get; set; }
        
        public required int UserID { get; set; }
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsAdmin { get; set; } = false;
        
        public virtual Group Group { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}