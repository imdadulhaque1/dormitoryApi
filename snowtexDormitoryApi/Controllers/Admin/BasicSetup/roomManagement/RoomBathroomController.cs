using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.commonFeature;
using snowtexDormitoryApi.Models.admin.basicSetup.roomManagements;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup.roomManagement
{
    [Route("api/admin/room/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class RoomBathroomController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomBathroomController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }


        // POST api/bathroom
        [HttpPost]
        public async Task<IActionResult> CreateBathroom([FromBody] CFPostRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid bathroom informations." });
            }

            if (!await UserExistsAsync(cfRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if Bathroom already exists by Bathroom Name
            var existingBathroom = await _context.roomBathroomModels.FirstOrDefaultAsync(r => r.name == cfRequest.name);
            if (existingBathroom != null)
            {
                return Conflict(new { status = 409, message = "Bathroom already exists." });
            }

            var newBathroom = new RoomBathroomSpecificationModel
            {
                name = cfRequest.name,
                remarks = cfRequest.remarks,
                createdBy = cfRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.roomBathroomModels.Add(newBathroom);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateBathroom), new
            {
                status = 201,
                message = "Bathroom created successfully!",
                Name = newBathroom.name
            });
        }

        // GET api/bed
        [HttpGet]
        public async Task<IActionResult> GetBed()
        {
            var bathrooms = await _context.roomBathroomModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Bathroom retrieved successfully.", data = bathrooms });
        }



        // PUT api/bathroom/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBathroom(int id, [FromBody] CFPutRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid bathroom info." });
            }
            if (!await UserExistsAsync(cfRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedBathroom = await _context.roomBathroomModels.FirstOrDefaultAsync(r => r.bathroomId == id && r.isActive == true);
            if (modifiedBathroom == null)
            {
                return NotFound(new { status = 404, message = "Bathroom not found or inactive." });
            }

            modifiedBathroom.name = cfRequest.name;
            modifiedBathroom.remarks = cfRequest.remarks;

            modifiedBathroom.updatedBy = cfRequest.updatedBy;
            modifiedBathroom.updatedTime = DateTime.UtcNow;

            _context.roomBathroomModels.Update(modifiedBathroom);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Bathroom informations updated successfully.", data = modifiedBathroom });
        }


        // DELETE api/bathroom/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBathroom(int id, [FromBody] CFDeleteRequestDto requestDto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(requestDto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Bathroom
            var deletedBathroom = await _context.roomBathroomModels.FindAsync(id);

            if (deletedBathroom == null || deletedBathroom.isActive == false)
            {
                return NotFound(new { status = 404, message = "Bathroom not found or already inactive" });
            }

            // Perform soft delete
            deletedBathroom.isActive = false;
            deletedBathroom.inactiveBy = requestDto.inactiveBy;
            deletedBathroom.inactiveDate = DateTime.UtcNow;

            _context.roomBathroomModels.Update(deletedBathroom);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Bathroom deleted successfully (soft delete)",
                data = deletedBathroom
            });
        }


    }
}
