using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.RoleBasedUserMenu
{
    public class RoleBasedUserModel
    {
        [Key]
        public int roleBasedUserId { get; set; }
        public required int roleId { get; set; }
        public required int userId { get; set; } // Single userId per record
        public required int createdBy { get; set; }
        public DateTime createdTime { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;
        public string? inactiveBy { get; set; }
        public bool isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public int? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
