namespace snowtexDormitoryApi.DTOs.admin.settings.newPerson
{
    public class postNewPersonDto
    {
        public required string name { get; set; }
        public required string companyName { get; set; }
        public required string personalPhoneNo { get; set; }
        public required string companyPhoneNo { get; set; }
        public required string email { get; set; }
        public required string nidBirthPassport { get; set; }
        public required string countryName { get; set; }
        public required string address { get; set; }

        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
