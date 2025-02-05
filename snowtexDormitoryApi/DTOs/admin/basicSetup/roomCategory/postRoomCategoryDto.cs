namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomCategory
{
    public class postRoomCategoryDto
    {
        public required string name { get; set; }
        public string? categoryBasedPrice { get; set; }
        public required string remarks { get; set; }

        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
