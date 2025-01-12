using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.basicSetup
{
    public class FloorInfoModel
    {
        [Key]
        public int floorId { get; set; }
        public required string floorName { get; set; }
        public string? floorDescription { get; set; }

        public int buildingId { get; set; }
        //public BuildingInfoModel? Building { get; set; } // Navigation property

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
