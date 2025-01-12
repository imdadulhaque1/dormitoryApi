namespace snowtexDormitoryApi.DTOs.admin.tags
{
    public class TagPostRequestDto
    {
        public string name { get; set; }
        public required string createdBy { get; set; }
    }

}
