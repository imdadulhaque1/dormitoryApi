namespace snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto
{
    public class deletePaidItemDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
    }
}
