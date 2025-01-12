using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.Models;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.DTOs;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class RoleController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public RoleController(AuthDbContext context)
        {
            _context = context;
        }

        // POST api/role
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequestDto roleRequest)
        {
            if (roleRequest == null || string.IsNullOrEmpty(roleRequest.name) || string.IsNullOrEmpty(roleRequest.createdBy))
            {
                return BadRequest(new { status = 400, message = "Invalid role data." });
            }

            // Check if role already exists by name
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.name == roleRequest.name);
            if (existingRole != null)
            {
                return Conflict(new { status = 409, message = "Role already exists." });
            }

            var newRole = new RoleModel
            {
                name = roleRequest.name,
                createdBy = roleRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateRole), new
            {
                status = 201,
                message = "Role created successfully!",
                Name = newRole.name
            });
        }

        // GET api/role
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(new { status = 200, message = "Roles retrieved successfully.", roles });
        }

        // GET api/role/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleId == id);
            if (role == null)
            {
                return NotFound(new { status = 404, message = "Role not found." });
            }

            return Ok(new { status = 200, message = "Role retrieved successfully.", role });
        }

        // PUT api/role/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateRequestDto roleRequest)
        {
            if (roleRequest == null || string.IsNullOrEmpty(roleRequest.name))
            {
                return BadRequest(new { status = 400, message = "Invalid role data." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleId == id);
            if (role == null)
            {
                return NotFound(new { status = 404, message = "Role not found." });
            }

            role.name = roleRequest.name;
            role.updatedBy = roleRequest.updatedBy;
            role.updatedTime = DateTime.UtcNow;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Role updated successfully.", role });
        }

        // DELETE api/role/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleId == id);
            if (role == null)
            {
                return NotFound(new { status = 404, message = "Role not found." });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Role deleted successfully." });
        }


    }
}
