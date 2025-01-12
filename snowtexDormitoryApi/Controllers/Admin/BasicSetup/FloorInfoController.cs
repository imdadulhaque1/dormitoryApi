using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.floorDto;
using snowtexDormitoryApi.Models.admin.basicSetup;
using System.Drawing;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class FloorInfoController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public FloorInfoController(AuthDbContext context)
        {
            _context = context;
        }

        // Helper Method: Check if User Exists
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId); // Replace `UserId` and `Users` with actual column and table names
        }

        // Create Floor
        [HttpPost("")]
        public async Task<IActionResult> CreateFloor(FloorPostRequestDto dto)
        {
            if (!await _context.buildingInfoModels.AnyAsync(b => b.buildingId == dto.buildingId && b.isActive == true ))
            {
                return StatusCode(500, new { status = 500, message = "Building not found or inactive" });
            }

            if (!await UserExistsAsync(dto.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var floor = new FloorInfoModel
            {
                floorName = dto.floorName,
                floorDescription = dto.floorDescription,
                buildingId = dto.buildingId,
                createdBy = dto.createdBy,
                createdTime = dto.createdTime ?? DateTime.Now
            };

            await _context.floorInfoModels.AddAsync(floor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFloorById), new { id = floor.floorId }, new
            {
                status = 201,
                message = "Floor created successfully",
                data = floor
            });
        }

        // Get All Floors
        //[HttpGet("")]
        //public async Task<IActionResult> GetFloors()
        //{
        //    var floors = await _context.floorInfoModels
        //        .Where(f => f.isActive == true)
        //        .ToListAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Fetched floors successfully",
        //        data = floors
        //    });
        //}

        // Get All Floors
        [HttpGet("")]
        public async Task<IActionResult> GetFloors()
        {
            var floors = await _context.floorInfoModels
                .Where(f => f.isActive == true)
                .Join(
                    _context.buildingInfoModels,  // The second table to join
                    floor => floor.buildingId,    // Foreign key in the floor table
                    building => building.buildingId, // Primary key in the building table
                    (floor, building) => new      // Project the result
                    {
                        floor.floorId,
                        floor.floorName,
                        floor.floorDescription,
                        floor.buildingId,
                        BuildingName = building.buildingName, // Include building name
                        floor.createdBy,
                        floor.createdTime,
                        floor.isActive
                    }
                )
                .ToListAsync();

            return Ok(new
            {
                status = 200,
                message = "Fetched floors successfully",
                data = floors
            });
        }


        // Get Floor by ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFloorById(int id)
        {
            var floor = await _context.floorInfoModels
                .FirstOrDefaultAsync(f => f.floorId == id && f.isActive == true);

            if (floor == null)
            {
                return NotFound(new { status = 404, message = "Floor not found" });
            }

            return Ok(new
            {
                status = 200,
                message = "Floor fetched successfully",
                data = floor
            });
        }

        // Update Floor
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFloor(int id, FloorPutRequestDto dto)
        {

            var floor = await _context.floorInfoModels.FirstOrDefaultAsync(f => f.floorId == id && f.isActive == true);

            if (floor == null)
            {
                return NotFound(new { status = 404, message = "Floor not found or inactive" });
            }

            if (!await _context.buildingInfoModels.AnyAsync(b => b.buildingId == dto.buildingId))
            {
                return StatusCode(500, new { status = 500, message = "Building not found" });
            }

            if (!await UserExistsAsync(dto.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            floor.floorName = dto.floorName;
            floor.floorDescription = dto.floorDescription;
            floor.buildingId = dto.buildingId;
            floor.updatedBy = dto.updatedBy;
            floor.updatedTime = dto.updatedTime ?? DateTime.Now;

            _context.floorInfoModels.Update(floor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Floor updated successfully",
                data = floor
            });
        }



        // Delete Floor (Soft Delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFloor(int id, [FromBody] DeleteFloorRequestDto dto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(dto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the floor
            var floor = await _context.floorInfoModels.FindAsync(id);

            if (floor == null || floor.isActive == false)
            {
                return NotFound(new { status = 404, message = "Floor not found or already inactive" });
            }

            // Perform soft delete
            floor.isActive = false;
            floor.inactiveBy = dto.inactiveBy;

            _context.floorInfoModels.Update(floor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Floor deleted successfully (soft delete)",
                data = floor
            });
        }

    }
}
