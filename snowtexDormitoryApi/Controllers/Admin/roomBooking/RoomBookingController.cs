using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto;
using snowtexDormitoryApi.DTOs.admin.roomBooking;
using snowtexDormitoryApi.Models.admin.basicSetup;
using snowtexDormitoryApi.Models.admin.roomBooking;
using System.Text.Json;

namespace snowtexDormitoryApi.Controllers.Admin.roomBooking
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomBookingController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public RoomBookingController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }


        [HttpPost]
        public async Task<IActionResult> CreateRoomBooking([FromBody] postRoomBookingDto postRequest)
        {
            // Validate the input data
            if (postRequest == null || postRequest.startTime == default || postRequest.endTime == default)
            {
                return BadRequest(new { status = 400, message = "Invalid room booking requirement's info." });
            }

            // Check if the room exists and is active
            if (!await _context.roomInfoModels.AnyAsync(b => b.roomId == postRequest.roomId && b.isActive == true))
            {
                return NotFound(new { status = 404, message = "Room not found or deleted" });
            }

            // Check if the user exists and is active
            if (!await _context.newPersonModels.AnyAsync(b => b.personId == postRequest.personId && b.isActive == true))
            {
                return NotFound(new { status = 404, message = "Applied user not found or deleted" });
            }

            // Check if the creator user exists
            if (!await UserExistsAsync(postRequest.createdBy))
            {
                return NotFound(new { status = 404, message = "Reference user not found" });
            }

            // Serialize paidItems and freeItems as JSON
            string paidItemsJson = postRequest.paidItems != null ? JsonSerializer.Serialize(postRequest.paidItems) : "[]";
            string freeItemsJson = postRequest.freeItems != null ? JsonSerializer.Serialize(postRequest.freeItems) : "[]";

            // Create a new RoomBookingModel
            var newRoomBooking = new RoomBookingModel
            {
                roomId = postRequest.roomId,
                personId = postRequest.personId,
                paidItems = paidItemsJson,  // Store JSON serialized paidItems
                freeItems = freeItemsJson,  // Store JSON serialized freeItems
                startTime = postRequest.startTime,
                endTime = postRequest.endTime,
                remarks = postRequest.remarks,
                createdBy = postRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            // Add the new room booking to the database
            _context.roomBookingModels.Add(newRoomBooking);
            await _context.SaveChangesAsync();

            // Return success response
            return CreatedAtAction(nameof(CreateRoomBooking), new
            {
                status = 201,
                message = "Room booking created successfully!",
                appliedPerson = newRoomBooking.personId
            });
        }


        // GET api/roomBooking
        [HttpGet]
        public async Task<IActionResult> FetchRoomBookingDetails(
        DateTime? startTime,
        DateTime? endTime,
        string? roomName,
        string? floorName,
        string? buildingName,
        string? categoryName,
        string? personName,
        string? personPhone)
            {
                try
                {
                    // Base query: Fetch active room bookings
                    var roomBookingsQuery = _context.roomBookingModels.Where(rb => rb.isActive == true);

                    // Apply date range filtering if provided
                    if (startTime.HasValue && endTime.HasValue)
                    {
                        roomBookingsQuery = roomBookingsQuery
                            .Where(rb => rb.startTime >= startTime.Value && rb.endTime <= endTime.Value);
                    }

                    var roomBookings = await roomBookingsQuery
                        .Select(rb => new
                        {
                            rb.roomBookingId,
                            rb.roomId,
                            rb.personId,
                            rb.startTime,
                            rb.endTime,
                            rb.remarks,
                            rb.isApprove,
                            rb.paidItems,
                            rb.freeItems,
                            rb.isActive,
                            rb.createdBy,
                            rb.updatedBy
                        })
                        .ToListAsync();

                    if (!roomBookings.Any())
                    {
                        return NotFound(new { status = 404, message = "No room bookings found." });
                    }

                    // Get unique room and person IDs
                    var roomIds = roomBookings.Select(rb => rb.roomId).Distinct().ToList();
                    var personIds = roomBookings.Select(rb => rb.personId).Distinct().ToList();

                    // Fetch room details in batch
                    var roomDetails = await _context.roomInfoModels
                        .Where(r => roomIds.Contains(r.roomId))
                        .ToListAsync();

                    var roomDetailsDict = roomDetails.ToDictionary(r => r.roomId, r => r);

                    // Fetch building, floor, and category details in batch
                    var buildingDetails = await _context.buildingInfoModels.ToListAsync();
                    var floorDetails = await _context.floorInfoModels.ToListAsync();
                    var categoryDetails = await _context.roomCategoryModels.ToListAsync();

                    var buildingDetailsDict = buildingDetails.ToDictionary(b => b.buildingId, b => b);
                    var floorDetailsDict = floorDetails.ToDictionary(f => f.floorId, f => f);
                    var categoryDetailsDict = categoryDetails.ToDictionary(c => c.roomCategoryId, c => c);

                    // Fetch person details in batch
                    var personDetails = await _context.newPersonModels
                        .Where(p => personIds.Contains(p.personId))
                        .ToListAsync();

                    var personDetailsDict = personDetails.ToDictionary(p => p.personId, p => p);

                    // Apply additional filtering based on optional parameters
                    var filteredRoomBookings = roomBookings
                        .Where(rb =>
                            (string.IsNullOrEmpty(roomName) || (roomDetailsDict.TryGetValue(rb.roomId, out var room) && room.roomName.Contains(roomName))) &&
                            (string.IsNullOrEmpty(buildingName) || (roomDetailsDict.TryGetValue(rb.roomId, out var r) &&
                                buildingDetailsDict.TryGetValue(r.buildingId, out var b) && b.buildingName.Contains(buildingName))) &&
                            (string.IsNullOrEmpty(floorName) || (roomDetailsDict.TryGetValue(rb.roomId, out var r1) &&
                                floorDetailsDict.TryGetValue(r1.floorId, out var f) && f.floorName.Contains(floorName))) &&
                            (string.IsNullOrEmpty(categoryName) || (roomDetailsDict.TryGetValue(rb.roomId, out var r2) &&
                                categoryDetailsDict.TryGetValue(r2.roomCategoryId, out var c) && c.name.Contains(categoryName))) &&
                            (string.IsNullOrEmpty(personName) || (personDetailsDict.TryGetValue(rb.personId, out var person) &&
                                person.name.Contains(personName))) &&
                            (string.IsNullOrEmpty(personPhone) || (personDetailsDict.TryGetValue(rb.personId, out var person2) &&
                                person2.personalPhoneNo.Contains(personPhone)))
                        )
                        .Select(rb => new
                        {
                            rb.roomBookingId,
                            rb.startTime,
                            rb.endTime,
                            rb.remarks,
                            rb.paidItems,
                            rb.freeItems,
                            rb.isActive,
                            rb.isApprove,
                            rb.createdBy,
                            rb.updatedBy,
                            rb.personId,
                            roomId = rb.roomId,
                            floorId = roomDetailsDict.TryGetValue(rb.roomId, out var room) ? room.floorId : (int?)null,
                            buildingId = roomDetailsDict.TryGetValue(rb.roomId, out room) ? room.buildingId : (int?)null,
                            categoryId = roomDetailsDict.TryGetValue(rb.roomId, out room) ? room.roomCategoryId : (int?)null,
                            roomName = room?.roomName,
                            buildingName = buildingDetailsDict.TryGetValue(room?.buildingId ?? 0, out var building) ? building.buildingName : null,
                            floorName = floorDetailsDict.TryGetValue(room?.floorId ?? 0, out var floor) ? floor.floorName : null,
                            categoryName = categoryDetailsDict.TryGetValue(room?.roomCategoryId ?? 0, out var category) ? category.name : null,
                            personDetails = personDetailsDict.TryGetValue(rb.personId, out var personDetailsObj) ? new
                            {
                                personDetailsObj.name,
                                personDetailsObj.email,
                                personDetailsObj.personalPhoneNo
                            } : null
                        })
                        .ToList();

                    return Ok(new { status = 200, message = "Room booking details retrieved successfully.", data = filteredRoomBookings });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { status = 500, message = "Internal server error.", error = ex.Message });
                }
            }

        [HttpPut]
        public async Task<IActionResult> UpdateRoomBooking(int bookingId, [FromBody] putRoomBookingDto putRequest)
        {
            // Validate input data
            if (putRequest == null || putRequest.startTime == default || putRequest.endTime == default)
            {
                return BadRequest(new { status = 400, message = "Invalid room booking update info." });
            }

            // Check if the booking exists
            var existingBooking = await _context.roomBookingModels.FindAsync(bookingId);
            if (existingBooking == null || existingBooking.isActive == false)
            {
                return NotFound(new { status = 404, message = "Room booking not found or inactive." });
            }

            // Check if the room exists and is active
            if (!await _context.roomInfoModels.AnyAsync(r => r.roomId == putRequest.roomId && r.isActive == true))
            {
                return NotFound(new { status = 404, message = "Room not found or deleted." });
            }

            // Check if the user exists and is active
            if (!await _context.newPersonModels.AnyAsync(p => p.personId == putRequest.personId && p.isActive == true))
            {
                return NotFound(new { status = 404, message = "User not found or deleted." });
            }

            // Check if the updating user exists
            if (!await UserExistsAsync(putRequest.updatedBy))
            {
                return NotFound(new { status = 404, message = "Updating reference user not found." });
            }

            // Serialize paidItems and freeItems as JSON
            string paidItemsJson = putRequest.paidItems ?? "[]";
            string freeItemsJson = putRequest.freeItems ?? "[]";

            // Update the existing room booking
            existingBooking.roomId = putRequest.roomId;
            existingBooking.personId = putRequest.personId;
            existingBooking.paidItems = paidItemsJson;
            existingBooking.freeItems = freeItemsJson;
            existingBooking.startTime = putRequest.startTime;
            existingBooking.endTime = putRequest.endTime;
            existingBooking.remarks = putRequest.remarks;
            existingBooking.updatedBy = putRequest.updatedBy;
            existingBooking.updatedTime = DateTime.UtcNow;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Room booking updated successfully!", bookingId = existingBooking.roomBookingId });
        }

        // DELETE api/roomBooking/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRoomBooking(int id, [FromBody] deleteRoomBookingDto deleteRequest)
        {
            // Validate if user exists
            if (!await UserExistsAsync(deleteRequest.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Booked Room
            var deletedBookedRoom= await _context.roomBookingModels.FindAsync(id);

            if (deletedBookedRoom == null || deletedBookedRoom.isActive == false)
            {
                return NotFound(new { status = 404, message = "Booked room not found or already inactive" });
            }

            // Perform soft delete
            deletedBookedRoom.isActive = false;
            deletedBookedRoom.inactiveBy = deleteRequest.inactiveBy;
            deletedBookedRoom.inactiveTime = DateTime.UtcNow;

            _context.roomBookingModels.Update(deletedBookedRoom);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Booked room deleted successfully",
                data = deletedBookedRoom
            });
        }


        [HttpGet("availableRoom")]
        public async Task<IActionResult> GetAvailableRooms(DateTime searchByStartTime, DateTime searchByEndTime)
        {
            try
            {
                if (searchByStartTime == default || searchByEndTime == default)
                {
                    return BadRequest(new { status = 400, message = "Invalid date range provided." });
                }

                var bookedRoomIds = await _context.roomBookingModels
                    .Where(rb =>
                        (searchByStartTime >= rb.startTime && searchByStartTime <= rb.endTime) ||
                        (searchByEndTime >= rb.startTime && searchByEndTime <= rb.endTime) ||
                        (rb.startTime >= searchByStartTime && rb.startTime <= searchByEndTime) ||
                        (rb.endTime >= searchByStartTime && rb.endTime <= searchByEndTime))
                    .Select(rb => rb.roomId)
                    .Distinct()
                    .ToListAsync();

                var availableRooms = await _context.roomInfoModels
                    .Where(r => !bookedRoomIds.Contains(r.roomId) && r.isActive == true)
                    .ToListAsync();

                if (!availableRooms.Any())
                {
                    return NotFound(new { status = 404, message = "No available rooms found for the selected time range." });
                }

                return Ok(new { status = 200, message = "Available rooms retrieved successfully.", data = availableRooms });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = "Internal server error.", error = ex.Message });
            }
        }


    }
}

