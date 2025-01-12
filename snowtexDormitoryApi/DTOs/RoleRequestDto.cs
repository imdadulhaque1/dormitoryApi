using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.DTOs
{
    public class RoleRequestDto
    {
        public string name { get; set; }
        public required string createdBy { get; set; }
    }
}
