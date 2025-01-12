using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.buildingDto;
using snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.commonFeature;
using snowtexDormitoryApi.Models.admin.basicSetup;
using snowtexDormitoryApi.Models.admin.basicSetup.roomManagements;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup.roomManagement
{
    [Route("api/admin/room/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class RoomCommonFeatureController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomCommonFeatureController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // POST api/BuildingInfo
        [HttpPost]
        public async Task<IActionResult> CreateCommonFeatures([FromBody] CFPostRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid Common features for room's data." });
            }

            if (!await UserExistsAsync(cfRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if building already exists by buildingName
            var existingCommonFeatures= await _context.roomCommonFeaturesModels.FirstOrDefaultAsync(r => r.name == cfRequest.name);
            if (existingCommonFeatures != null)
            {
                return Conflict(new { status = 409, message = "Common features already exists." });
            }

            var newCommonfeatures = new RoomCommonFeaturesModel
            {
                name = cfRequest.name,
                remarks = cfRequest.remarks,
                createdBy = cfRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.roomCommonFeaturesModels.Add(newCommonfeatures);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateCommonFeatures), new
            {
                status = 201,
                message = "Common features created successfully!",
                Name = newCommonfeatures.name
            });
        }

        // GET api/RoomCommonFeatures
        [HttpGet]
        public async Task<IActionResult> GetCommonFeatures()
        {
            var buildings = await _context.roomCommonFeaturesModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Common Features retrieved successfully.", data = buildings });
        }


        // PUT api/RoomCommonFeatures/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommonFeatures(int id, [FromBody] CFPutRequestDto cfRequest)
        {
            if (cfRequest == null || string.IsNullOrEmpty(cfRequest.name) || string.IsNullOrEmpty(cfRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid Common features data." });
            }
            if (!await UserExistsAsync(cfRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedCF = await _context.roomCommonFeaturesModels.FirstOrDefaultAsync(r => r.commonFeatureId == id && r.isActive == true);
            if (modifiedCF == null)
            {
                return NotFound(new { status = 404, message = "Common features not found or inactive." });
            }

            modifiedCF.name = cfRequest.name;
            modifiedCF.remarks = cfRequest.remarks;

            modifiedCF.updatedBy = cfRequest.updatedBy;
            modifiedCF.updatedTime = DateTime.UtcNow;

            _context.roomCommonFeaturesModels.Update(modifiedCF);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Common features updated successfully.", data = modifiedCF });
        }


        // DELETE api/CommonFeatures/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCommonFeatures(int id, [FromBody] CFDeleteRequestDto requestDto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(requestDto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Common features
            var deletedCF = await _context.roomCommonFeaturesModels.FindAsync(id);

            if (deletedCF == null || deletedCF.isActive == false)
            {
                return NotFound(new { status = 404, message = "Common features not found or already inactive" });
            }

            // Perform soft delete
            deletedCF.isActive = false;
            deletedCF.inactiveBy = requestDto.inactiveBy;
            deletedCF.inactiveDate = DateTime.UtcNow;

            _context.roomCommonFeaturesModels.Update(deletedCF);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Common Features deleted successfully (soft delete)",
                data = deletedCF
            });
        }

    
    }
}
