namespace Fichte.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}