namespace snowtexDormitoryApi.DTOs.admin.tags
{
    public class TagPutRequestDto
    {
        public string name { get; set; }
        public required string updatedBy { get; set; }
        public DateTime? updatedTime { get; set; } = DateTime.UtcNow;
    }
}
