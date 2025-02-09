namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomDto
{
    public class RoomPutRequestDto
    {
        public required string roomName { get; set; }
        public string? roomDescription { get; set; }
        public string? remarks { get; set; }
        public int roomCategoryId { get; set; }
        public int floorId { get; set; }
        public int buildingId { get; set; }
        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }

    }
}
