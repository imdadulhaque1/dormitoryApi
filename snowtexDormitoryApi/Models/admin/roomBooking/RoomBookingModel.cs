using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.roomBooking
{
    public class RoomBookingModel
    {
        [Key]
        public int roomBookingId { get; set; }
        public int roomId { get; set; }
        public int personId { get; set; }

        public string? paidItems { get; set; }
        public string? freeItems { get; set; }
        public required DateTime startTime { get; set; }
        public required DateTime endTime { get; set; }

        public string? remarks { get; set; } 

        public bool? isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public DateTime? approvedTime { get; set; }
        public bool? isActive { get; set; } = true;
        public int? inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
        public int? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
