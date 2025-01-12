using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.DTOs.admin.basicSetup.buildingDto
{
    public class BuildingPostRequestDto
    {
        public required string buildingName { get; set; }
        public required string buildingAddress { get; set; }

        [Phone]
        public required string contactNo { get; set; }
        public required int createdBy { get; set; }
    }
}
