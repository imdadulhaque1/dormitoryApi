using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.floorDto;
using snowtexDormitoryApi.DTOs.admin.basicSetup.roomDto;
using snowtexDormitoryApi.Models.admin.basicSetup;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class RoomInfoController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomInfoController(AuthDbContext context)
        {
            _context = context;
        }

        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // Create Floor
        [HttpPost("")]
        public async Task<IActionResult> CreateRoom(RoomPostRequestDto dto)
        {
            if (!await _context.buildingInfoModels.AnyAsync(b => b.buildingId == dto.buildingId && b.isActive == true))
            {
                return StatusCode(500, new { status = 404, message = "Building not found or inactive" });
            }

            if (!await _context.floorInfoModels.AnyAsync(b => b.floorId == dto.floorId && b.isActive == true))
            {
                return StatusCode(500, new { status = 404, message = "Floor not found or inactive" });
            }

            if (!await UserExistsAsync(dto.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var newRoom = new RoomInfoModel
            {
                roomName = dto.roomName,
                roomDescription = dto.roomDescription,
                floorId = dto.floorId,
                buildingId = dto.buildingId,
                createdBy = dto.createdBy,
                createdTime = dto.createdTime ?? DateTime.Now
            };

            await _context.roomInfoModels.AddAsync(newRoom);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.roomId }, new
            {
                status = 201,
                message = "Room created successfully",
                data = newRoom
            });
        }

        // Get All Rooms
        [HttpGet]
        //[Route("api/admin/RoomInfo")]
        public async Task<IActionResult> GetRooms(
        string? name = null,
        int? buildingId = null,
        string? buildingName = null,
        string? floorName = null,
        string? sortBy = null,
        string? sortOrder = "asc",
        int page = 1,
        int pageSize = 10)
        {
            // Validate page and pageSize values
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Base query for active rooms
            var query = _context.roomInfoModels
                .Where(r => r.isActive == true)
                .Join(
                    _context.buildingInfoModels,
                    room => room.buildingId,
                    building => building.buildingId,
                    (room, building) => new
                    {
                        room,
                        BuildingName = building.buildingName
                    }
                )
                .Join(
                    _context.floorInfoModels,
                    roomBuilding => roomBuilding.room.floorId,
                    floor => floor.floorId,
                    (roomBuilding, floor) => new
                    {
                        roomBuilding.room.roomId,
                        roomBuilding.room.roomName,
                        roomBuilding.room.roomDescription,
                        roomBuilding.room.buildingId,
                        roomBuilding.BuildingName,
                        roomBuilding.room.floorId,
                        FloorName = floor.floorName,
                        roomBuilding.room.createdBy,
                        roomBuilding.room.createdTime,
                        roomBuilding.room.isActive
                    }
                );

            // Apply optional filters based on query parameters
            if (!string.IsNullOrEmpty(name))
            {
                var lowerName = name.ToLower();
                query = query.Where(r => r.roomName.ToLower().Contains(lowerName));
            }

            if (buildingId.HasValue)
            {
                query = query.Where(r => r.buildingId == buildingId.Value);
            }

            if (!string.IsNullOrEmpty(buildingName))
            {
                var lowerBuildingName = buildingName.ToLower();
                query = query.Where(r => r.BuildingName.ToLower().Contains(lowerBuildingName));
            }

            if (!string.IsNullOrEmpty(floorName))
            {
                var lowerFloorName = floorName.ToLower();
                query = query.Where(r => r.FloorName.ToLower().Contains(lowerFloorName));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                    : query.OrderBy(e => EF.Property<object>(e, sortBy));
            }

            // Get total count before applying pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var rooms = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return result
            return Ok(new
            {
                status = 200,
                message = "Fetched rooms successfully",
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = rooms
            });
        }



        //[HttpGet("")]
        //public async Task<IActionResult> GetRooms(
        //string? searchKeyword = null,
        //string? sortBy = null,
        //string? sortOrder = "asc",
        //int page = 1,
        //int pageSize = 10)
        //{
        //    // Validate page and pageSize values
        //    if (page < 1) page = 1;
        //    if (pageSize < 1) pageSize = 10;

        //    // Base query for active rooms
        //    var query = _context.roomInfoModels
        //        .Where(r => r.isActive == true)
        //        .Join(
        //            _context.buildingInfoModels,
        //            room => room.buildingId,
        //            building => building.buildingId,
        //            (room, building) => new
        //            {
        //                room,
        //                BuildingName = building.buildingName
        //            }
        //        )
        //        .Join(
        //            _context.floorInfoModels,
        //            roomBuilding => roomBuilding.room.floorId,
        //            floor => floor.floorId,
        //            (roomBuilding, floor) => new
        //            {
        //                roomBuilding.room.roomId,
        //                roomBuilding.room.roomName,
        //                roomBuilding.room.roomDescription,
        //                roomBuilding.room.buildingId,
        //                roomBuilding.BuildingName,
        //                roomBuilding.room.floorId,
        //                FloorName = floor.floorName,
        //                roomBuilding.room.createdBy,
        //                roomBuilding.room.createdTime,
        //                roomBuilding.room.isActive
        //            }
        //        );

        //    // Apply search filter if keyword is provided
        //    if (!string.IsNullOrEmpty(searchKeyword))
        //    {
        //        searchKeyword = searchKeyword.ToLower();
        //        query = query.Where(r =>
        //            r.roomName.ToLower().Contains(searchKeyword) ||
        //            r.roomDescription.ToLower().Contains(searchKeyword) ||
        //            r.BuildingName.ToLower().Contains(searchKeyword) ||
        //            r.FloorName.ToLower().Contains(searchKeyword)
        //        );
        //    }

        //    // Apply sorting
        //    if (!string.IsNullOrEmpty(sortBy))
        //    {
        //        query = sortOrder.ToLower() == "desc"
        //            ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
        //            : query.OrderBy(e => EF.Property<object>(e, sortBy));
        //    }

        //    // Get total count before applying pagination
        //    var totalCount = await query.CountAsync();

        //    // Apply pagination
        //    var rooms = await query
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    // Return result
        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Fetched rooms successfully",
        //        totalCount,
        //        page,
        //        pageSize,
        //        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        //        data = rooms
        //    });
        //}

        //[HttpGet("")]
        //public async Task<IActionResult> GetRooms()
        //{
        //    var rooms = await _context.roomInfoModels
        //        .Where(r => r.isActive == true)
        //        .Join(
        //            _context.buildingInfoModels,      // Join with buildingInfoModels
        //            room => room.buildingId,
        //            building => building.buildingId,
        //            (room, building) => new
        //            {
        //                room,
        //                BuildingName = building.buildingName
        //            }
        //        )
        //        .Join(
        //            _context.floorInfoModels,        // Join with floorInfoModels
        //            roomBuilding => roomBuilding.room.floorId,
        //            floor => floor.floorId,
        //            (roomBuilding, floor) => new
        //            {
        //                roomBuilding.room.roomId,
        //                roomBuilding.room.roomName,
        //                roomBuilding.room.roomDescription,
        //                roomBuilding.room.buildingId,
        //                roomBuilding.BuildingName,
        //                roomBuilding.room.floorId,
        //                FloorName = floor.floorName,
        //                roomBuilding.room.createdBy,
        //                roomBuilding.room.createdTime,
        //                roomBuilding.room.isActive
        //            }
        //        )
        //        .ToListAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Fetched rooms successfully",
        //        data = rooms
        //    });
        //}

        //[HttpGet("")]
        //public async Task<IActionResult> GetRooms()
        //{
        //    var rooms = await _context.roomInfoModels
        //        .Where(f => f.isActive == true)
        //        .ToListAsync();

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "Fetched rooms successfully",
        //        data = rooms
        //    });
        //}


        // Get Room by ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var floor = await _context.roomInfoModels
                .FirstOrDefaultAsync(f => f.roomId == id && f.isActive == true);

            if (floor == null)
            {
                return NotFound(new { status = 404, message = "Room not found" });
            }

            return Ok(new
            {
                status = 200,
                message = "Room fetched successfully",
                data = floor
            });
        }


        // Update Floor
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRoom(int id, RoomPutRequestDto dto)
        {
            var room = await _context.roomInfoModels.FirstOrDefaultAsync(f => f.roomId == id && f.isActive == true);

            if (room == null)
            {
                return NotFound(new { status = 404, message = "Room not found or inactive" });
            }

            if (!await _context.buildingInfoModels.AnyAsync(b => b.buildingId == dto.buildingId))
            {
                return StatusCode(500, new { status = 500, message = "Building not found" });
            }

            if (!await _context.floorInfoModels.AnyAsync(b => b.floorId == dto.floorId))
            {
                return StatusCode(500, new { status = 500, message = "Floor not found" });
            }

            if (!await UserExistsAsync(dto.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            room.roomName = dto.roomName;
            room.roomDescription= dto.roomDescription;
            room.floorId = dto.floorId;
            room.buildingId = dto.buildingId;
            room.updatedBy = dto.updatedBy;
            room.updatedTime = dto.updatedTime ?? DateTime.Now;

            _context.roomInfoModels.Update(room);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Room updated successfully",
                data = room
            });
        }

        // Delete Floor (Soft Delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRoom(int id, [FromBody] RoomDeleteRequestDto dto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(dto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the floor
            var room = await _context.roomInfoModels.FindAsync(id);

            if (room == null || room.isActive == false)
            {
                return NotFound(new { status = 404, message = "Room not found or already inactive" });
            }

            // Perform soft delete
            room.isActive = false;
            room.inactiveBy = dto.inactiveBy;

            _context.roomInfoModels.Update(room);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Room deleted successfully (soft delete)",
                data = room
            });
        }




    }
}
