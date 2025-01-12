using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.RoleBasedUserMenu
{
   public class RoleBasedMenuModel
    {
        [Key]
        public int roleBasedMenuId { get; set; }
        public required int roleId { get; set; }
        public required int menuId { get; set; } // Single menuId per record
        public required int createdBy { get; set; }
        public DateTime createdTime { get; set; } = DateTime.UtcNow; 
        public bool isActive { get; set; } = true; 
        public string? inactiveBy { get; set; } 
        public bool isApprove { get; set; } = false; 
        public string? approvedBy { get; set; }
        public string? updatedBy { get; set; } 
        public DateTime? updatedTime { get; set; }
    }

}
