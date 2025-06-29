namespace Fichte.Dtos
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserID { get; set; }
        public int MaxMembers { get; set; }
        public bool IsPrivate { get; set; }
        public int MemberCount { get; set; }
        public bool IsUserMember { get; set; }
        public bool IsUserAdmin { get; set; }
    }
}