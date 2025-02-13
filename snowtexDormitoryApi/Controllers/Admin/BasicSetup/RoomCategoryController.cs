using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.roomCategory;
using snowtexDormitoryApi.DTOs.admin.settings.newPerson;
using snowtexDormitoryApi.Models.admin.basicSetup;
using snowtexDormitoryApi.Models.admin.settings;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomCategoryController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomCategoryController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // POST api/roomCategory
        [HttpPost]
        public async Task<IActionResult> CreateRoomCategory([FromBody] postRoomCategoryDto postRequest)
        {
            if (postRequest == null || string.IsNullOrEmpty(postRequest.name) || string.IsNullOrEmpty(postRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid informations to create a new room category." });
            }

            if (!await UserExistsAsync(postRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if the category is already exists by roomCategory_Name
            var existingRoomCategory= await _context.roomCategoryModels.FirstOrDefaultAsync(r => r.name == postRequest.name && r.isActive == true);
            if (existingRoomCategory != null)
            {
                return Conflict(new { status = 409, message = "Room category already exists." });
            }

            var newRoomCategory = new RoomCategoryModel
            {
                name = postRequest.name,
                categoryBasedPrice = postRequest.categoryBasedPrice,
                noOfPerson = postRequest.noOfPerson,
                remarks = postRequest.remarks,
                createdBy = postRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.roomCategoryModels.Add(newRoomCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateRoomCategory), new
            {
                status = 201,
                message = "Room category created successfully!",
                Name = newRoomCategory.name
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomCategories()
        {
            var roomCategories = await _context.roomCategoryModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Room categories retrieved successfully.", data = roomCategories });
        }


        // PUT api/roomCategory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomCategory(int id, [FromBody] putRoomCategoryDto putRequest)
        {
            if (putRequest == null || string.IsNullOrEmpty(putRequest.name) || string.IsNullOrEmpty(putRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid informations to update room's category" });
            }
            if (!await UserExistsAsync(putRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedRoomCategory= await _context.roomCategoryModels.FirstOrDefaultAsync(r => r.roomCategoryId == id && r.isActive == true);
            if (modifiedRoomCategory == null)
            {
                return NotFound(new { status = 404, message = "Room category not found or inactive." });
            }

            modifiedRoomCategory.name = putRequest.name;
            modifiedRoomCategory.categoryBasedPrice = putRequest.categoryBasedPrice;
            modifiedRoomCategory.noOfPerson = putRequest.noOfPerson;
            modifiedRoomCategory.remarks = putRequest.remarks;

            modifiedRoomCategory.updatedBy = putRequest.updatedBy;
            modifiedRoomCategory.updatedTime = DateTime.UtcNow;

            _context.roomCategoryModels.Update(modifiedRoomCategory);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Room category updated successfully.", data = modifiedRoomCategory });
        }

        // DELETE api/roomCategory/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRoomCategory(int id, [FromBody] deleteRoomCategoryDto deleteRequest)
        {
            // Validate if user exists
            if (!await UserExistsAsync(deleteRequest.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the roomCategory by roomCategoryId
            var deletedRoomCategory = await _context.roomCategoryModels.FindAsync(id);

            if (deletedRoomCategory == null || deletedRoomCategory.isActive == false)
            {
                return NotFound(new { status = 404, message = "Room category not found or already inactive" });
            }

            // Perform soft delete
            deletedRoomCategory.isActive = false;
            deletedRoomCategory.inactiveBy = deleteRequest.inactiveBy;
            deletedRoomCategory.inactiveTime = DateTime.UtcNow;

            _context.roomCategoryModels.Update(deletedRoomCategory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Room category deleted successfully (soft delete)",
                data = deletedRoomCategory
            });
        }



    }
}
