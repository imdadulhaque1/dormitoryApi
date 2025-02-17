using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto;
using snowtexDormitoryApi.DTOs.admin.roomBooking;
using snowtexDormitoryApi.Models.admin.basicSetup;
using snowtexDormitoryApi.Models.admin.roomBooking;
using snowtexDormitoryApi.Models.admin.settings;
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


            // Check if the creator user exists
            if (!await UserExistsAsync(postRequest.createdBy))
            {
                return NotFound(new { status = 404, message = "Reference user not found" });
            }

            // Create a new RoomBookingModel
            var newRoomBooking = new RoomBookingModel
            {
                personInfo = postRequest.personInfo,
                roomInfo = postRequest.roomInfo,
                paidItems = postRequest.paidItems,
                freeItems = postRequest.freeItems,
                totalFreeItemsPrice = postRequest.totalFreeItemsPrice,
                totalPaidItemsPrice = postRequest.totalPaidItemsPrice,
                totalRoomPrice = postRequest.totalRoomPrice,
                grandTotal = postRequest.grandTotal,
                totalDays = postRequest.totalDays,
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
                appliedPerson = newRoomBooking.roomInfo
            });
        }


        // GET api/roomBooking
        [HttpGet]
        public async Task<IActionResult> GetBookedRoom()
        {
            var bookedRoom = await _context.roomBookingModels
                .Where(f => f.isActive == true)
                .Select(f => new
                {
                    f.roomBookingId,
                    f.personInfo,
                    f.roomInfo, 
                    f.paidItems,
                    f.freeItems,
                    totalPaidItemsPrice = (double?)f.totalPaidItemsPrice,
                    totalFreeItemsPrice = (double?)f.totalFreeItemsPrice,
                    totalRoomPrice = (double?)f.totalRoomPrice,
                    grandTotal = (double?)f.grandTotal,
                    f.totalDays,
                    f.startTime,
                    f.endTime,
                    f.remarks,
                    f.isApprove,
                    f.approvedBy,
                    f.approvedTime,
                    f.isActive,
                    f.inactiveBy,
                    f.inactiveTime,
                    f.createdBy,
                    f.createdTime,
                    f.updatedBy,
                    f.updatedTime
                })
                .ToListAsync();

            return Ok(new { status = 200, message = "Booking room retrieved successfully.", data = bookedRoom });
        }

        

        //[HttpPut]
        //public async Task<IActionResult> UpdateRoomBooking(int bookingId, [FromBody] putRoomBookingDto putRequest)
        //{
        //    // Validate input data
        //    if (putRequest == null || putRequest.startTime == default || putRequest.endTime == default)
        //    {
        //        return BadRequest(new { status = 400, message = "Invalid room booking update info." });
        //    }

        //    // Check if the booking exists
        //    var existingBooking = await _context.roomBookingModels.FindAsync(bookingId);
        //    if (existingBooking == null || existingBooking.isActive == false)
        //    {
        //        return NotFound(new { status = 404, message = "Room booking not found or inactive." });
        //    }

        //    // Check if the room exists and is active
        //    if (!await _context.roomInfoModels.AnyAsync(r => r.roomId == putRequest.roomId && r.isActive == true))
        //    {
        //        return NotFound(new { status = 404, message = "Room not found or deleted." });
        //    }

        //    // Check if the user exists and is active
        //    if (!await _context.newPersonModels.AnyAsync(p => p.personId == putRequest.personId && p.isActive == true))
        //    {
        //        return NotFound(new { status = 404, message = "User not found or deleted." });
        //    }

        //    // Check if the updating user exists
        //    if (!await UserExistsAsync(putRequest.updatedBy))
        //    {
        //        return NotFound(new { status = 404, message = "Updating reference user not found." });
        //    }

        //    // Serialize paidItems and freeItems as JSON
        //    string paidItemsJson = putRequest.paidItems ?? "[]";
        //    string freeItemsJson = putRequest.freeItems ?? "[]";

        //    // Update the existing room booking
        //    existingBooking.roomId = putRequest.roomId;
        //    existingBooking.personId = putRequest.personId;
        //    existingBooking.paidItems = paidItemsJson;
        //    existingBooking.freeItems = freeItemsJson;
        //    existingBooking.startTime = putRequest.startTime;
        //    existingBooking.endTime = putRequest.endTime;
        //    existingBooking.remarks = putRequest.remarks;
        //    existingBooking.updatedBy = putRequest.updatedBy;
        //    existingBooking.updatedTime = DateTime.UtcNow;

        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return Ok(new { status = 200, message = "Room booking updated successfully!", bookingId = existingBooking.roomBookingId });
        //}

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
                if (searchByStartTime == default || searchByEndTime == default || searchByStartTime >= searchByEndTime)
                {
                    return BadRequest(new { status = 400, message = "Invalid date range provided." });
                }

                // Step 1: Fetch booked room JSON data within the given time range
                var bookedRoomsJson = await _context.roomBookingModels
                    .Where(rb => rb.isActive == true &&
                        (searchByStartTime < rb.endTime && searchByEndTime > rb.startTime))
                    .Select(rb => rb.roomInfo)
                    .ToListAsync();

                var bookedRoomIds = new HashSet<int>();

                foreach (var json in bookedRoomsJson)
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            // Deserialize into the correct structure
                            var rooms = JsonConvert.DeserializeObject<List<RoomBookingDto>>(json);

                            foreach (var booking in rooms)
                            {
                                if (booking?.roomInfo?.roomId.HasValue == true)
                                {
                                    bookedRoomIds.Add(booking.roomInfo.roomId.Value);
                                }
                            }
                        }
                        catch (Newtonsoft.Json.JsonException ex)
                        {
                            Console.WriteLine($"JSON Parse Error: {ex.Message}"); // Log JSON parsing errors
                        }
                    }
                }

                // Step 2: Get available rooms that are not booked in the time range
                var availableRooms = await _context.roomInfoModels
                    .Where(r => r.isActive == true && r.isRoomAvailable == true && !bookedRoomIds.Contains(r.roomId))
                    .Select(r => new
                    {
                        r.roomId,
                        r.roomName,
                        r.roomDescription,
                        r.remarks,
                        r.roomCategoryId,
                        r.floorId,
                        r.buildingId,
                        r.isRoomAvailable,
                        r.haveRoomDetails,
                        r.isApprove,
                        r.isActive,
                        r.inactiveBy,
                        r.createdBy,
                        r.createdTime,
                        r.updatedBy,
                        r.updatedTime,
                        Floor = _context.floorInfoModels.FirstOrDefault(f => f.floorId == r.floorId),
                        Building = _context.buildingInfoModels.FirstOrDefault(b => b.buildingId == r.buildingId),
                        RoomCategory = _context.roomCategoryModels.FirstOrDefault(rc => rc.roomCategoryId == r.roomCategoryId)
                    }).ToListAsync();

                if (!availableRooms.Any())
                {
                    return NotFound(new { status = 404, message = "No available rooms found for the selected time range." });
                }

                var response = availableRooms.Select(room => new
                {
                    room.roomId,
                    room.roomName,
                    room.roomDescription,
                    room.remarks,
                    room.roomCategoryId,
                    room.floorId,
                    room.buildingId,
                    room.isRoomAvailable,
                    room.haveRoomDetails,
                    room.isApprove,
                    room.isActive,
                    room.inactiveBy,
                    room.createdBy,
                    room.createdTime,
                    room.updatedBy,
                    room.updatedTime,
                    FloorName = room.Floor?.floorName,
                    BuildingName = room.Building?.buildingName,
                    RoomCategoryName = room.RoomCategory?.name,
                    roomWisePerson = room.RoomCategory?.noOfPerson,
                    roomPrice = room.RoomCategory?.categoryBasedPrice
                });
                // Step 3: Return the available rooms
                return Ok(new { status = 200, message = "Available rooms retrieved successfully.", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = "Internal server error.", error = ex.Message });
            }
        }

        public class RoomBookingDto
        {
            public RoomInfoDto roomInfo { get; set; }
        }

        public class RoomInfoDto
        {
            public int? roomId { get; set; }
        }

    }
}

