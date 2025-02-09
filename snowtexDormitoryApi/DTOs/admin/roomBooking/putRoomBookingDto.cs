namespace snowtexDormitoryApi.DTOs.admin.roomBooking
{
    public class putRoomBookingDto
    {
        public int roomId { get; set; }
        public int personId { get; set; }
        public required string paidItems { get; set; }
        public required string freeItems { get; set; }
        public required DateTime startTime { get; set; }
        public required DateTime endTime { get; set; }
        public string? remarks { get; set; }

        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
