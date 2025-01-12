using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.RoleBasedUserMenu;
using snowtexDormitoryApi.Models.admin.RoleBasedUserMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace snowtexDormitoryApi.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class RoleBasedMenuController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public RoleBasedMenuController(AuthDbContext context)
        {
            _context = context;
        }

        // POST: api/RoleBasedMenu
        [HttpPost]
        public async Task<IActionResult> CreateRelationBetweenRoleAndMenu([FromBody] RoleBasedMenuRequestDto roleMenuRequest)
        {
            // Validate request
            if (!IsValidRequest(roleMenuRequest, out var validationError))
            {
                return BadRequest(new { status = 400, message = validationError });
            }

            // Verify if the role exists
            var roleExists = await RoleExists(roleMenuRequest.roleId);
            if (!roleExists)
            {
                return NotFound(new { status = 404, message = $"Role with ID {roleMenuRequest.roleId} not found." });
            }

            // Verify if all provided menuIds exist
            var validMenuIds = await GetValidMenuIds(roleMenuRequest.menuIds);
            var invalidMenuIds = roleMenuRequest.menuIds.Except(validMenuIds).ToList();

            // Track created and failed relations using anonymous objects
            var createdRelations = new List<object>();
            var failedToCreateRelations = new List<object>();

            foreach (var menuId in roleMenuRequest.menuIds)
            {
                var relation = new RoleBasedMenuModel
                {
                    roleId = roleMenuRequest.roleId,
                    menuId = menuId,
                    createdBy = roleMenuRequest.createdBy,
                    createdTime = DateTime.UtcNow,
                    isActive = true,
                    isApprove = false,
                    inactiveBy = null,
                    approvedBy = null,
                    updatedBy = null,
                    updatedTime = null
                };

                // Check if the menuId is already related to the role
                var existingRelation = await _context.RoleBasedMenus
                    .AnyAsync(r => r.roleId == roleMenuRequest.roleId && r.menuId == menuId);

                if (!existingRelation && validMenuIds.Contains(menuId))
                {
                    // Add the valid relation to createdRelations list
                    createdRelations.Add(new
                    {
                        roleId = relation.roleId,
                        menuId = relation.menuId,
                        createdBy = relation.createdBy,
                        createdTime = relation.createdTime
                    });
                }
                else
                {
                    // Add the failed relation to failedToCreateRelations list
                    failedToCreateRelations.Add(new
                    {
                        roleId = relation.roleId,
                        menuId = relation.menuId,
                        createdBy = relation.createdBy,
                        createdTime = relation.createdTime
                    });
                }
            }

            // Save valid relations to the database
            if (createdRelations.Any())
            {
                // Extract menuIds from the createdRelations list
                var newMenuIds = createdRelations
                    .Where(f => validMenuIds.Contains((int)((dynamic)f).menuId)) // Use dynamic to access the menuId
                    .Select(f => (int)((dynamic)f).menuId) // Extract the menuId
                    .ToList();

                if (newMenuIds.Any())
                {
                    await AddRoleMenuRelations(roleMenuRequest.roleId, newMenuIds, roleMenuRequest.createdBy);
                }
            }

            return CreatedAtAction(nameof(CreateRelationBetweenRoleAndMenu), new
            {
                status = 201,
                message = "Role-menu relationships created successfully.",
                createdRelations = createdRelations,
                failedToCreateRelations = failedToCreateRelations
            });
        }

        // Helper method: Validate request
        private bool IsValidRequest(RoleBasedMenuRequestDto request, out string validationError)
        {
            if (request == null)
            {
                validationError = "Request cannot be null.";
                return false;
            }

            if (request.roleId <= 0)
            {
                validationError = "Invalid roleId.";
                return false;
            }

            if (request.menuIds == null || !request.menuIds.Any())
            {
                validationError = "menuIds cannot be null or empty.";
                return false;
            }

            if (request.createdBy <= 0)
            {
                validationError = "createdBy cannot be null or empty.";
                return false;
            }

            validationError = string.Empty;
            return true;
        }

        // Helper method: Check if a role exists
        private async Task<bool> RoleExists(int roleId)
        {
            return await _context.Roles.AnyAsync(r => r.roleId == roleId);
        }

        // Helper method: Get valid menuIds
        private async Task<List<int>> GetValidMenuIds(List<int> menuIds)
        {
            return await _context.Menus
                .Where(m => menuIds.Contains(m.menuId))
                .Select(m => m.menuId)
                .ToListAsync();
        }

        // Helper method: Add role-menu relations
        private async Task AddRoleMenuRelations(int roleId, List<int> menuIds, int createdBy)
        {
            var newRelations = menuIds.Select(menuId => new RoleBasedMenuModel
            {
                roleId = roleId,
                menuId = menuId,
                createdBy = createdBy,
                createdTime = DateTime.UtcNow,
                isActive = true,
                isApprove = false,
                inactiveBy = null,
                approvedBy = null,
                updatedBy = null,
                updatedTime = null
            }).ToList();

            await _context.RoleBasedMenus.AddRangeAsync(newRelations);
            await _context.SaveChangesAsync();
        }

        // GET: api/RoleBasedMenu/{roleId}
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetMenuByRole(int roleId)
        {
            // Verify if the role exists
            var roleExists = await RoleExists(roleId);
            if (!roleExists)
            {
                return NotFound(new { status = 404, message = $"Role with ID {roleId} not found." });
            }

            // Retrieve all menus associated with the role
            var roleMenus = await _context.RoleBasedMenus
                .Where(rbm => rbm.roleId == roleId)
                .Select(rbm => new
                {
                    rbm.roleId,
                    rbm.menuId,
                    rbm.createdBy,
                    rbm.createdTime,
                    rbm.isActive,
                    rbm.isApprove
                })
                .ToListAsync();

            if (roleMenus.Count == 0)
            {
                return NotFound(new { status = 404, message = "No menus found for this role." });
            }

            return Ok(new { status = 200, roleId, menus = roleMenus });
        }

        // DELETE: api/RoleBasedMenu/{roleId}/{menuId}
        [HttpDelete("{roleId}/{menuId}")]
        public async Task<IActionResult> DeleteMenuFromRole(int roleId, int menuId)
        {
            // Verify if the role exists
            var roleExists = await RoleExists(roleId);
            if (!roleExists)
            {
                return NotFound(new { status = 404, message = $"Role with ID {roleId} not found." });
            }

            // Verify if the menu exists for the given role
            var existingRelation = await _context.RoleBasedMenus
                .FirstOrDefaultAsync(rbm => rbm.roleId == roleId && rbm.menuId == menuId);

            if (existingRelation == null)
            {
                return NotFound(new { status = 404, message = "No such role-menu relationship exists." });
            }

            _context.RoleBasedMenus.Remove(existingRelation);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Role-menu relationship deleted successfully." });
        }
    }
}
