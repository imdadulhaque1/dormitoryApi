using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models
{
    public class TagModel
    {
        [Key]
        public int tagId { get; set; }
        public required string name { get; set; }
        public bool isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public bool isActive { get; set; } = true;
        public string? createdBy { get; set; }
        public required DateTime createdTime { get; set; } = DateTime.UtcNow;
        public string? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }

    }
}
