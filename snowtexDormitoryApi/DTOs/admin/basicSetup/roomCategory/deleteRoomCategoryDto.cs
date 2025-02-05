namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomCategory
{
    public class deleteRoomCategoryDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
    }
}
