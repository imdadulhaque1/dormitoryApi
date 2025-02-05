namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomCategory
{
    public class putRoomCategoryDto
    {
        public required string name { get; set; }
        public string? categoryBasedPrice { get; set; }
        public required string remarks { get; set; }

        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
