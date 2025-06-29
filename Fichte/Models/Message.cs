using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fichte.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IDMessage { get; set; }
        public required string Body { get; set; }
        
        public required int UserID { get; set; }
        
        public int? GroupID { get; set; }
        
        public int? RecipientUserID { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [JsonIgnore]
        public virtual Group? Group { get; set; }

        [JsonIgnore]
        public virtual User? RecipientUser { get; set; }
    }
}