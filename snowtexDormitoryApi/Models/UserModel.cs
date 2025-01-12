using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models
{
    public class UserModel
    {
        [Key]
        public int userId { get; set; }

        [MaxLength(100)]
        public required string name { get; set; }

        [MaxLength(100)]
        public required string companyName { get; set; }

        [EmailAddress]
        public required string email { get; set; }

        [Phone]
        public required string phone { get; set; }

        [MinLength(6)]
        public required string password { get; set; }

        public required int accountType { get; set; }

        public string? token { get; set; }

        public required DateTime createdDate { get; set; } = DateTime.UtcNow;

        public DateTime? updatedDate { get; set; }

        public bool isActive { get; set; } = true;
    }
}
