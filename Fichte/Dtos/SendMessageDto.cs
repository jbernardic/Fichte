using System.ComponentModel.DataAnnotations;

namespace Fichte.Dtos
{
    public class SendMessageDto
    {
        [Required(ErrorMessage = "Message body is required.")]
        public required string Body { get; set; }
        
        public int? GroupID { get; set; }
        
        public int? RecipientUserID { get; set; }
    }
}