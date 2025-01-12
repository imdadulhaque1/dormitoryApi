using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.DTOs.admin.basicSetup.floorDto
{
    public class FloorPostRequestDto
    {
        public required string floorName { get; set; }
        public string? floorDescription { get; set; }
        public int buildingId { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
