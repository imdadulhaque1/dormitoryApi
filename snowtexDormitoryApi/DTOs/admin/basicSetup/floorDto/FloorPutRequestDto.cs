namespace snowtexDormitoryApi.DTOs.admin.basicSetup.floorDto
{
    public class FloorPutRequestDto
    {
        public required string floorName { get; set; }
        public string? floorDescription { get; set; }
        public int buildingId { get; set; }
        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
