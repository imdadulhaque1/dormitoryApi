using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.basicSetup
{
    public class RoomCategoryModel
    {
        [Key]
        public int roomCategoryId { get; set; }
        public required string name { get; set; }
        public string? categoryBasedPrice { get; set; }
        public required string remarks { get; set; }
        public int? noOfPerson { get; set; }

        public bool? isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public bool? isActive { get; set; } = true;
        public int? inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
        public int? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
