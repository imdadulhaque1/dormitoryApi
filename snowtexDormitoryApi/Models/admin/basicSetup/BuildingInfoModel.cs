using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.basicSetup
{
    public class BuildingInfoModel
    {
        [Key]
        public int buildingId { get; set; }
        public required string buildingName { get; set; }
        public required string buildingAddress { get; set; }

        [Phone]
        public required string contactNo { get; set; }

        public bool? isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public bool? isActive { get; set; } = true;
        public int? inactiveBy { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
        public int? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
