namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.roomDetails
{
    public class RoomDetailsDeleteRequestDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveDate { get; set; }
    }
}
