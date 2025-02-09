namespace snowtexDormitoryApi.DTOs.admin.roomBooking
{
    public class deleteRoomBookingDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
    }
}
