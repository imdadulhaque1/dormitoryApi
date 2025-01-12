using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs;
using snowtexDormitoryApi.DTOs.admin.basicSetup.buildingDto;
using snowtexDormitoryApi.DTOs.admin.basicSetup.floorDto;
using snowtexDormitoryApi.Models;
using snowtexDormitoryApi.Models.admin.basicSetup;
using System.Drawing;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class BuildingInfoController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public BuildingInfoController(AuthDbContext context)
        {
            _context = context;
        }

        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // POST api/BuildingInfo
        [HttpPost]
        public async Task<IActionResult> CreateBuilding([FromBody] BuildingPostRequestDto buildingRequest)
        {
            if (buildingRequest == null || string.IsNullOrEmpty(buildingRequest.buildingName) || string.IsNullOrEmpty(buildingRequest.buildingAddress) || string.IsNullOrEmpty(buildingRequest.contactNo))
            {
                return BadRequest(new { status = 400, message = "Invalid Building data." });
            }

            if (!await UserExistsAsync(buildingRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if building already exists by buildingName
            var existingBuildings= await _context.buildingInfoModels.FirstOrDefaultAsync(r => r.buildingName == buildingRequest.buildingName);
            if (existingBuildings != null)
            {
                return Conflict(new { status = 409, message = "Building already exists." });
            }

            var newBuilding= new BuildingInfoModel
            {
                buildingName = buildingRequest.buildingName,
                buildingAddress = buildingRequest.buildingAddress,
                contactNo = buildingRequest.contactNo,
                createdBy = buildingRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.buildingInfoModels.Add(newBuilding);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateBuilding), new
            {
                status = 201,
                message = "Building created successfully!",
                Name = newBuilding.buildingName
            });
        }

        // GET api/BuildingInfo
        [HttpGet]
        public async Task<IActionResult> GetBuildings()
        {
            var buildings = await _context.buildingInfoModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Buildings retrieved successfully.", data=buildings });
        }

        // PUT api/BuildingInfo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuildingInfo(int id, [FromBody] BuildingPutRequestDto buildingRequest)
        {
            if (buildingRequest == null || string.IsNullOrEmpty(buildingRequest.buildingName) || string.IsNullOrEmpty(buildingRequest.buildingAddress) || string.IsNullOrEmpty(buildingRequest.contactNo))
            {
                return BadRequest(new { status = 400, message = "Invalid building data." });
            }
            if (!await UserExistsAsync(buildingRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedBuilding = await _context.buildingInfoModels.FirstOrDefaultAsync(r => r.buildingId == id && r.isActive == true);
            if (modifiedBuilding == null)
            {
                return NotFound(new { status = 404, message = "Building not found on inactive." });
            }

            modifiedBuilding.buildingName = buildingRequest.buildingName;
            modifiedBuilding.buildingAddress = buildingRequest.buildingAddress;
            modifiedBuilding.contactNo = buildingRequest.contactNo;

            modifiedBuilding.updatedBy = buildingRequest.updatedBy;
            modifiedBuilding.updatedTime = DateTime.UtcNow;

            _context.buildingInfoModels.Update(modifiedBuilding);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Building updated successfully.", data = modifiedBuilding });
        }






        // DELETE api/BuildingInfo/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBuilding(int id, [FromBody] BuildingDeleteRequestDto dto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(dto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the floor
            var deletedBuilding = await _context.buildingInfoModels.FindAsync(id);

            if (deletedBuilding == null || deletedBuilding.isActive == false)
            {
                return NotFound(new { status = 404, message = "Building not found or already inactive" });
            }

            // Perform soft delete
            deletedBuilding.isActive = false;
            deletedBuilding.inactiveBy = dto.inactiveBy;

            _context.buildingInfoModels.Update(deletedBuilding);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Buildings deleted successfully (soft delete)",
                data = deletedBuilding
            });
        }



    }
}
