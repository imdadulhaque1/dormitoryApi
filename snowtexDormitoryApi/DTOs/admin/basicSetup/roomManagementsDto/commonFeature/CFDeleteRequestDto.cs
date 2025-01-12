namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.commonFeature
{
    public class CFDeleteRequestDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveDate { get; set; }
    }
}
