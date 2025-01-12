namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.commonFeature
{
    public class CFPostRequestDto
    {
        public required string name { get; set; }
        public required string remarks { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
