using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.DTOs.admin.basicSetup.buildingDto
{
    public class BuildingPutRequestDto
    {
        public required string buildingName { get; set; }
        public required string buildingAddress { get; set; }
        [Phone]
        public required string contactNo { get; set; }
        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; } = DateTime.UtcNow;
    }
}
