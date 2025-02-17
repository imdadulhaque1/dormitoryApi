namespace snowtexDormitoryApi.DTOs.admin.roomBooking
{
    public class putRoomBookingDto
    {
        public required string personInfo { get; set; }
        public required string roomInfo { get; set; }
        public string? paidItems { get; set; }
        public string? freeItems { get; set; }
        public float? totalPaidItemsPrice { get; set; }
        public float? totalFreeItemsPrice { get; set; }
        public float? totalRoomPrice { get; set; }
        public float? grandTotal { get; set; }
        public required int totalDays { get; set; }
        public required DateTime startTime { get; set; }
        public required DateTime endTime { get; set; }
        public string? remarks { get; set; }

        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
