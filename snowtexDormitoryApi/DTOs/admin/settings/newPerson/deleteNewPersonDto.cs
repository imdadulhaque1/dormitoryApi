namespace snowtexDormitoryApi.DTOs.admin.settings.newPerson
{
    public class deleteNewPersonDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
    }
}
