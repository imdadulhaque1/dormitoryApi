using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace snowtexDormitoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class UsersController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public UsersController(AuthDbContext context)
        {
            _context = context;
        }

        // GET api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 10)
        {
            // Validate page and pageSize
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;  // Limit max page size for performance reasons

            // Calculate the total number of users
            var totalUsers = await _context.Users.CountAsync();

            // Fetch the users for the given page
            var users = await _context.Users
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(user => new
                                       {
                                           user.userId,
                                           user.name,
                                           user.email,
                                           user.phone,
                                           user.accountType,
                                           user.createdDate,
                                           user.updatedDate
                                       })
                                       .ToListAsync();

            // Return paginated response
            return Ok(new
            {
                status = 200,
                message = "Users retrieved successfully.",
                totalUsers,
                page,
                pageSize,
                users
            });
        }
    }
}


