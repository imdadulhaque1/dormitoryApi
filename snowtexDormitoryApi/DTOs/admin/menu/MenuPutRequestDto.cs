namespace snowtexDormitoryApi.DTOs.admin.menu
{
    public class MenuPutRequestDto
    {
        public required string banglaName { get; set; }
        public required string englishName { get; set; }
        public required string url { get; set; }
        public required string parentLayerId { get; set; }
        public required int menuSerialNo { get; set; }
        public required string htmlIcon { get; set; }
        public required string updatedBy { get; set; }
        public DateTime? updatedTime { get; set; } = DateTime.UtcNow;
    }
}
