using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.roomBooking
{
    public class RoomBookingModel
    {
        [Key]
        public int roomBookingId { get; set; }
        public required string personInfo { get; set; }
        public required string roomInfo { get; set; }
        public string? paidItems { get; set; }
        public string? freeItems { get; set; }
        public double? totalPaidItemsPrice { get; set; }
        public double? totalFreeItemsPrice { get; set; }
        public double? totalRoomPrice { get; set; }
        public double? grandTotal { get; set; }
        public required int totalDays { get; set; }
        public required DateTime startTime { get; set; }
        public required DateTime endTime { get; set; }

        public string? remarks { get; set; } 

        public bool? isApprove { get; set; } = false;
        public int? approvedBy { get; set; }
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
