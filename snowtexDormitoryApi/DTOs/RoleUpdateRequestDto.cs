namespace snowtexDormitoryApi.DTOs
{
    public class RoleUpdateRequestDto
    {
        public string name { get; set; }
        public required string updatedBy { get; set; }
        public DateTime? updatedTime { get; set; } = DateTime.UtcNow;
    }
}
