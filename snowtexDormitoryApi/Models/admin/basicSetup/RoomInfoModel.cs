using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.basicSetup
{
    public class RoomInfoModel
    {
        [Key]
        public int roomId { get; set; }
        public required string roomName { get; set; }
        public string? roomDescription { get; set; }
        public int floorId { get; set; }
        public int buildingId { get; set; }

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
