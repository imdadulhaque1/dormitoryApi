namespace snowtexDormitoryApi.DTOs.admin.menu
{
    public class MenuPostRequestDto
    {
        public required string banglaName { get; set; }
        public required string englishName { get; set; }
        public string url { get; set; }
        public required string parentLayerId { get; set; }
        public required int menuSerialNo { get; set; }
        public required string htmlIcon { get; set; }
        public required string createdBy { get; set; }
    }
}
