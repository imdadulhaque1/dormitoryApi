namespace snowtexDormitoryApi.DTOs.admin.RoleBasedUserMenu
{
    public class RoleBasedUserRequestDto
    {
        public required int roleId { get; set; }
        public required List<int> userIds { get; set; }
        public required int createdBy { get; set; }
    }
}
