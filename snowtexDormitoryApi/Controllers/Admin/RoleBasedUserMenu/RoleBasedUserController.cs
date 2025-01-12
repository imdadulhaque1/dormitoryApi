using Microsoft.AspNetCore.Mvc;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.Models.admin.RoleBasedUserMenu;
using snowtexDormitoryApi.DTOs.admin.RoleBasedUserMenu;
using snowtexDormitoryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace snowtexDormitoryApi.Controllers.Admin.RoleBasedUserMenu
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class RoleBasedUserController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public RoleBasedUserController(AuthDbContext context)
        {
            _context = context;
        }

        // POST: api/RoleBasedUser
        [HttpPost]
        public async Task<IActionResult> CreateRelationBetweenRoleAndUser([FromBody] RoleBasedUserRequestDto roleUserRequest)
        {
            // Validate request
            if (!IsValidRequest(roleUserRequest, out var validationError))
            {
                return BadRequest(new { status = 400, message = validationError });
            }

            // Verify if the role exists
            var roleExists = await RoleExists(roleUserRequest.roleId);
            if (!roleExists)
            {
                return NotFound(new { status = 404, message = $"Role with ID {roleUserRequest.roleId} not found." });
            }

            // Get valid userIds
            var validUserIds = await GetValidUserIds(roleUserRequest.userIds);
            var invalidUserIds = roleUserRequest.userIds.Except(validUserIds).ToList();

            // Track successful and failed relations
            var createdRelations = new List<object>();
            var failedToCreateRelations = invalidUserIds.Select(userId => new
            {
                roleId = roleUserRequest.roleId,
                userId = userId,
                message = $"User with ID {userId} not found."
            }).ToList();

            // Remove all existing relations for the role
            var existingRelations = await _context.RoleBasedUsers
                .Where(r => r.roleId == roleUserRequest.roleId)
                .ToListAsync();

            _context.RoleBasedUsers.RemoveRange(existingRelations);

            // Insert only the latest `userIds`
            foreach (var userId in validUserIds)
            {
                var newRelation = new RoleBasedUserModel
                {
                    roleId = roleUserRequest.roleId,
                    userId = userId,
                    createdBy = roleUserRequest.createdBy,
                    createdTime = DateTime.UtcNow,
                    isActive = true,
                    isApprove = false,
                    inactiveBy = null,
                    approvedBy = null,
                    updatedBy = null,
                    updatedTime = null
                };

                await _context.RoleBasedUsers.AddAsync(newRelation);

                createdRelations.Add(new
                {
                    roleId = newRelation.roleId,
                    userId = newRelation.userId,
                    createdBy = newRelation.createdBy,
                    createdTime = newRelation.createdTime,
                    message = "New relation created."
                });
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateRelationBetweenRoleAndUser), new
            {
                status = 201,
                message = "Role-user relationships processed successfully. Only the latest `userIds` are stored.",
                createdRelations = createdRelations,
                failedToCreateRelations = failedToCreateRelations
            });
        }

        // Helper method: Validate request
        private bool IsValidRequest(RoleBasedUserRequestDto request, out string validationError)
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

            if (request.userIds == null || !request.userIds.Any())
            {
                validationError = "userIds cannot be null or empty.";
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

        // Helper method: Get valid userIds
        private async Task<List<int>> GetValidUserIds(List<int> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.userId))
                .Select(u => u.userId)
                .ToListAsync();
        }


        [HttpGet]
        [HttpGet("{roleId?}")]
        public async Task<IActionResult> GetUsersByRole(int? roleId = null)
        {
            if (roleId == null)
            {
                // Fetch all users when no roleId is provided
                var allUsers = await _context.Users
                    .Select(u => new
                    {
                        u.userId,
                        u.name, // User's name
                        u.email, // User's email
                        roleId = _context.RoleBasedUsers
                            .Where(r => r.userId == u.userId)
                            .Select(r => r.roleId)
                            .FirstOrDefault() // Single roleId for each user
                    })
                    .ToListAsync();

                return Ok(new
                {
                    status = 200,
                    message = "All users retrieved successfully.",
                    users = allUsers
                });
            }

            // Check if the role exists
            if (!await RoleExists(roleId.Value))
            {
                return NotFound(new { status = 404, message = $"Role with ID {roleId} not found." });
            }

            // Fetch users associated with the specific role
            var users = await _context.RoleBasedUsers
                .Where(r => r.roleId == roleId.Value)
                .Join(_context.Users,
                    rb => rb.userId, // Foreign key in RoleBasedUsers
                    u => u.userId,   // Primary key in Users
                    (rb, u) => new
                    {
                        u.userId,
                        u.name,   // User's name
                        u.email,  // User's email
                        rb.roleId // Single roleId for the user
                    })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound(new { status = 404, message = $"No users found for Role ID {roleId}." });
            }

            return Ok(new
            {
                status = 200,
                message = "Users retrieved successfully for the given role.",
                roleId = roleId,
                users = users
            });
        }



        [HttpGet("/api/admin/getUserBasedMenu")]
        public async Task<IActionResult> GetMenusByUserId(int userId)
        {
            // Validate the userId
            var userExists = await _context.Users.AnyAsync(u => u.userId == userId);
            if (!userExists)
            {
                return NotFound(new
                {
                    status = 404,
                    message = $"User with ID {userId} not found."
                });
            }

            // Fetch roles for the user
            var userRoles = await _context.RoleBasedUsers
                .Where(rbu => rbu.userId == userId && rbu.isActive)
                .Select(rbu => rbu.roleId)
                .ToListAsync();

            // Determine if the user has roleId 1 or 2
            bool hasFullAccessRole = userRoles.Contains(1) || userRoles.Contains(2);

            // Fetch menus based on roles and active relationships
            List<UserBasedRequestDto> menuData;

            if (hasFullAccessRole)
            {
                // Fetch all menus if user has full access roles
                menuData = await _context.Menus
                    .Where(menu => menu.isActive == true) // Explicitly check for true to handle nullable bool
                    .OrderBy(menu => menu.menuSerialNo) // Sort by menuSerialNo
                    .Select(menu => new UserBasedRequestDto
                    {
                        menuId = menu.menuId,
                        banglaName = menu.banglaName,
                        englishName = menu.englishName,
                        url = menu.url,
                        parentLayerId = menu.parentLayerId,
                        menuSerialNo = menu.menuSerialNo,
                        htmlIcon = menu.htmlIcon
                    })
                    .ToListAsync();
            }
            else
            {
                // Fetch menus based on user's specific roles
                menuData = await (from rbu in _context.RoleBasedUsers
                                  join rbm in _context.RoleBasedMenus on rbu.roleId equals rbm.roleId
                                  join menu in _context.Menus on rbm.menuId equals menu.menuId
                                  where rbu.userId == userId && rbu.isActive && rbm.isActive
                                  orderby menu.menuSerialNo // Sort by menuSerialNo
                                  select new UserBasedRequestDto
                                  {
                                      menuId = menu.menuId,
                                      banglaName = menu.banglaName,
                                      englishName = menu.englishName,
                                      url = menu.url,
                                      parentLayerId = menu.parentLayerId,
                                      menuSerialNo = menu.menuSerialNo,
                                      htmlIcon = menu.htmlIcon
                                  }).ToListAsync();
            }

            if (!menuData.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "No menus found for the specified user."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Menus fetched successfully based on user!",
                data = new
                {
                    userId,
                    menus = menuData
                }
            });
        }


    }
}
