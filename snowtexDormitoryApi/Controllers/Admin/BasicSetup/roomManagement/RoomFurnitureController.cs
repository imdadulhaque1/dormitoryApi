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
    public class RoomFurnitureController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomFurnitureController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // POST api/AvailableFurniture
        [HttpPost]
        public async Task<IActionResult> CreateAvailableFurniture([FromBody] CFPostRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid furnitures information." });
            }

            if (!await UserExistsAsync(cfRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if furniture already exists by furniture Name
            var existingFurniture = await _context.roomAFModels.FirstOrDefaultAsync(r => r.name == cfRequest.name);
            if (existingFurniture != null)
            {
                return Conflict(new { status = 409, message = "Furniture already exists." });
            }

            var newFuniture = new RoomAvailableFurnitureModel
            {
                name = cfRequest.name,
                remarks = cfRequest.remarks,
                createdBy = cfRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.roomAFModels.Add(newFuniture);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateAvailableFurniture), new
            {
                status = 201,
                message = "Furniture created successfully!",
                Name = newFuniture.name
            });
        }

        // GET api/AvailableFurniture
        [HttpGet]
        public async Task<IActionResult> GetAvailableFurniture()
        {
            var furnitures = await _context.roomAFModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Furnitures retrieved successfully.", data = furnitures });
        }



        // PUT api/AvailableFurniture/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAvailableFurniture(int id, [FromBody] CFPutRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid furniture info." });
            }
            if (!await UserExistsAsync(cfRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedAF = await _context.roomAFModels.FirstOrDefaultAsync(r => r.availableFurnitureId == id && r.isActive == true);
            if (modifiedAF == null)
            {
                return NotFound(new { status = 404, message = "Furnitutre not found or inactive." });
            }

            modifiedAF.name = cfRequest.name;
            modifiedAF.remarks = cfRequest.remarks;

            modifiedAF.updatedBy = cfRequest.updatedBy;
            modifiedAF.updatedTime = DateTime.UtcNow;

            _context.roomAFModels.Update(modifiedAF);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Furnitutre informations updated successfully.", data = modifiedAF });
        }


        // DELETE api/AvailableFurniture/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAvailableFurniture(int id, [FromBody] CFDeleteRequestDto requestDto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(requestDto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Available furnitutre
            var deletedAF = await _context.roomAFModels.FindAsync(id);

            if (deletedAF == null || deletedAF.isActive == false)
            {
                return NotFound(new { status = 404, message = "Furniture not found or already inactive" });
            }

            // Perform soft delete
            deletedAF.isActive = false;
            deletedAF.inactiveBy = requestDto.inactiveBy;
            deletedAF.inactiveDate = DateTime.UtcNow;

            _context.roomAFModels.Update(deletedAF);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Furniture deleted successfully (soft delete)",
                data = deletedAF
            });
        }



    }
}
