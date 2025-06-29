using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Fichte.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IDUser { get; set; }
        public required string Username { get; set; }
        public string? PasswordHash { get; set; }
        
        public string? Email { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSeen { get; set; }
        public bool IsOnline { get; set; } = false;
        
        public string? GoogleId { get; set; }
        public string? GitHubId { get; set; }
        public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }

}
