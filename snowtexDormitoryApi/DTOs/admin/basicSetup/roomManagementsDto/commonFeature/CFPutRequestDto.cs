namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.commonFeature
{
    public class CFPutRequestDto
    {
        public required string name { get; set; }
        public required string remarks { get; set; }
        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
