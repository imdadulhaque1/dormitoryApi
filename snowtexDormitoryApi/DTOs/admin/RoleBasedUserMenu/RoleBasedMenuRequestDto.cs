namespace snowtexDormitoryApi.DTOs.admin.RoleBasedUserMenu
{
    public class RoleBasedMenuRequestDto
    {
        public required int roleId { get; set; }
        public required List<int> menuIds { get; set; }
        public required int createdBy { get; set; }
    }
}
