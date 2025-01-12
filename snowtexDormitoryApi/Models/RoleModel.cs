using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models
{
    public class RoleModel
    {
        [Key]
        public int roleId { get; set; }
        public string name { get; set; }
        public bool? isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public bool? isActive { get; set; } = true;
        public  required string createdBy { get; set; }
        public DateTime? createdTime { get; set; }
        public string? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }

}
