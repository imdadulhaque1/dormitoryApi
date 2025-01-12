using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.menu
{
    public class MenuModel
    {
        
        [Key]
        public int menuId { get; set; }
        public required int menuSerialNo { get; set; }
        //public int selfLayerId { get; set; }
        public required string banglaName { get; set; }
        public required string englishName { get; set; }
        public string url { get; set; }
        public required string parentLayerId { get; set; }
        public required string htmlIcon { get; set; }

        public bool? isApprove { get; set; } = false;
        public string? approvedBy { get; set; }
        public bool? isActive { get; set; } = true;
        public string? inactiveBy { get; set; }
        public required string createdBy { get; set; }
        public DateTime? createdTime { get; set; }
        public string? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    
    }
}
