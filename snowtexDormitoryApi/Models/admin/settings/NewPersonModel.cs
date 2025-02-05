using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.settings
{
    public class NewPersonModel
    {
        [Key]
        public int personId { get; set; }
        public required string name { get; set; }
        public required string companyName { get; set; }
        public required string personalPhoneNo { get; set; }
        public required string companyPhoneNo { get; set; }
        public required string email { get; set; }
        public required string nidBirthPassport { get; set; }
        public required string countryName { get; set; }
        public required string address { get; set; }

        public bool? isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public bool? isActive { get; set; } = true;
        public int? inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
        public int? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }

    }
}
