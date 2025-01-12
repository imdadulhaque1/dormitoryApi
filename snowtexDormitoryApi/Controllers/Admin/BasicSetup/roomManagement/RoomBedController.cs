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
    public class RoomBedController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomBedController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }


        // POST api/bed
        [HttpPost]
        public async Task<IActionResult> CreateBed([FromBody] CFPostRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid bed informations." });
            }

            if (!await UserExistsAsync(cfRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if bed already exists by bed Name
            var existingBed = await _context.roomBedModels.FirstOrDefaultAsync(r => r.name == cfRequest.name);
            if (existingBed != null)
            {
                return Conflict(new { status = 409, message = "Bed already exists." });
            }

            var newBed = new RoomBedSpecificationModel
            {
                name = cfRequest.name,
                remarks = cfRequest.remarks,
                createdBy = cfRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.roomBedModels.Add(newBed);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateBed), new
            {
                status = 201,
                message = "Bed created successfully!",
                Name = newBed.name
            });
        }

        // GET api/bed
        [HttpGet]
        public async Task<IActionResult> GetBed()
        {
            var beds = await _context.roomBedModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Bed retrieved successfully.", data = beds });
        }



        // PUT api/bed/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBed(int id, [FromBody] CFPutRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid bed informations." });
            }
            if (!await UserExistsAsync(cfRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedBed = await _context.roomBedModels.FirstOrDefaultAsync(r => r.bedId == id && r.isActive == true);
            if (modifiedBed == null)
            {
                return NotFound(new { status = 404, message = "Bed not found or inactive." });
            }

            modifiedBed.name = cfRequest.name;
            modifiedBed.remarks = cfRequest.remarks;

            modifiedBed.updatedBy = cfRequest.updatedBy;
            modifiedBed.updatedTime = DateTime.UtcNow;

            _context.roomBedModels.Update(modifiedBed);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Bed informations updated successfully.", data = modifiedBed });
        }


        // DELETE api/bed/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBed(int id, [FromBody] CFDeleteRequestDto requestDto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(requestDto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Bed
            var deletedBed = await _context.roomBedModels.FindAsync(id);

            if (deletedBed == null || deletedBed.isActive == false)
            {
                return NotFound(new { status = 404, message = "Bed not found or already inactive" });
            }

            // Perform soft delete
            deletedBed.isActive = false;
            deletedBed.inactiveBy = requestDto.inactiveBy;
            deletedBed.inactiveDate = DateTime.UtcNow;

            _context.roomBedModels.Update(deletedBed);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Bed deleted successfully (soft delete)",
                data = deletedBed
            });
        }


    }
}
