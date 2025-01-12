namespace snowtexDormitoryApi.DTOs
{
    public class SignupRequestDto
    {
        public string name { get; set; }
        public string companyName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string password { get; set; }
        public int accountType { get; set; }
    }
}
