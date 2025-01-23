using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.commonFeature;
using snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.roomDetails;
using snowtexDormitoryApi.Models.admin.basicSetup.roomManagements;
using System.IO;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup.roomManagement
{
    [Route("api/admin/room/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomDetailsController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly string _imageDirectory = @"D:\Kamal Vai\dotNet\snowtexDormitoryApi\snowtexDormitoryApi\images";

        public RoomDetailsController(AuthDbContext context)
        {
            _context = context;
        }

        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        private async Task<string> GetNameByIdAsync(int id, string entity)
        {
            switch (entity.ToLower())
            {
                case "building":
                    return await _context.buildingInfoModels.Where(b => b.buildingId == id)
                                                   .Select(b => b.buildingName)
                                                   .FirstOrDefaultAsync();
                case "floor":
                    return await _context.floorInfoModels.Where(f => f.floorId == id)
                                                .Select(f => f.floorName)
                                                .FirstOrDefaultAsync();
                case "room":
                    return await _context.roomInfoModels.Where(r => r.roomId == id)
                                               .Select(r => r.roomName)
                                               .FirstOrDefaultAsync();
                default:
                    throw new ArgumentException("Invalid entity type specified.");
            }
        }

        // POST api/admin/room/roomdetails
        [HttpPost]
        public async Task<IActionResult> CreateRoomDetails([FromBody] RoomDetailsPostRequestDto roomDetailsRequest)
        {
            if (!IsValidRoomDetails(roomDetailsRequest))
            {
                return BadRequest(new { status = 400, message = "Invalid room details." });
            }

            if (!await UserExistsAsync(roomDetailsRequest.createdBy))
            {
                return NotFound(new { status = 404, message = "User not found." });
            }

            var existingRoom = await _context.roomDetailsModels
                .FirstOrDefaultAsync(r => r.roomId == roomDetailsRequest.roomId &&
                                          r.floorId == roomDetailsRequest.floorId &&
                                          r.buildingId == roomDetailsRequest.buildingId &&
                                          r.isActive == true);
            if (existingRoom != null)
            {
                return Conflict(new { status = 409, message = "Room details already exist." });
            }

            var buildingName = await GetNameByIdAsync(roomDetailsRequest.buildingId, "building");
            var floorName = await GetNameByIdAsync(roomDetailsRequest.floorId, "floor");
            var roomName = await GetNameByIdAsync(roomDetailsRequest.roomId, "room");

            if (string.IsNullOrEmpty(buildingName) || string.IsNullOrEmpty(floorName) || string.IsNullOrEmpty(roomName))
            {
                return NotFound(new { status = 404, message = "Invalid building, floor, or room ID." });
            }

            var imageUrls = await ProcessImagesAsync(roomDetailsRequest.roomImages, buildingName, floorName, roomName, roomDetailsRequest.createdBy);

            var newRoomDetails = new RoomDetailsModel
            {
                roomId = roomDetailsRequest.roomId,
                floorId = roomDetailsRequest.floorId,
                buildingId = roomDetailsRequest.buildingId,
                roomDimension = roomDetailsRequest.roomDimension,
                roomSideId = roomDetailsRequest.roomSideId,
                bedSpecificationId = roomDetailsRequest.bedSpecificationId,
                roomBelconiId = roomDetailsRequest.roomBelconiId,
                attachedBathroomId = roomDetailsRequest.attachedBathroomId,
                commonFeatures = roomDetailsRequest.commonFeatures,
                availableFurnitures = roomDetailsRequest.availableFurnitures,
                //bedSpecification = roomDetailsRequest.bedSpecification,
                bathroomSpecification = roomDetailsRequest.bathroomSpecification,
                roomImages = imageUrls,
                createdBy = roomDetailsRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true,
                
            };

            _context.roomDetailsModels.Add(newRoomDetails);

            // Update the haveRoomDetails property in RoomInfoModel
            var roomInfo = await _context.roomInfoModels
                .FirstOrDefaultAsync(r => r.roomId == roomDetailsRequest.roomId &&
                                          r.floorId == roomDetailsRequest.floorId &&
                                          r.buildingId == roomDetailsRequest.buildingId &&
                                          r.isActive == true);

            if (roomInfo == null)
            {
                return NotFound(new { status = 404, message = "Room not found for updating haveRoomDetails properties." });
            }

            roomInfo.haveRoomDetails = true;
            _context.roomInfoModels.Update(roomInfo);

            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(CreateRoomDetails), new
            {
                status = 201,
                message = "Room details created successfully!",
                roomDetailsId = newRoomDetails.roomDetailsId
            });
        }

        private bool IsValidRoomDetails(RoomDetailsPostRequestDto roomDetailsRequest)
        {
            return roomDetailsRequest != null &&
                   !string.IsNullOrEmpty(roomDetailsRequest.roomDimension) &&
                   roomDetailsRequest.roomId > 0 &&
                   roomDetailsRequest.floorId > 0 &&
                   roomDetailsRequest.buildingId > 0;
        }

        private async Task<List<string>> ProcessImagesAsync(List<string> roomImages, string buildingName, string floorName, string roomName, int createdBy)
        {
            var imageUrls = new List<string>();

            if (roomImages != null && roomImages.Any())
            {
                foreach (var base64Image in roomImages)
                {
                    var imageUrl = await SaveImageAsync(base64Image, buildingName, floorName, roomName, createdBy);
                    imageUrls.Add(imageUrl);
                }
            }

            return imageUrls;
        }

        private async Task<string> SaveImageAsync(string base64Image, string buildingName, string floorName, string roomName, int createdBy)
        {
            var currentDateTime = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueNo = Guid.NewGuid().ToString().Substring(0, 8);

            var fileName = $"{currentDateTime}_{createdBy}_{uniqueNo}.png";
            // var directoryPath = Path.Combine(_imageDirectory, $"{buildingName}/{floorName}/{roomName}");
            //var directoryPath = _imageDirectory;

            //if (!Directory.Exists(directoryPath))
            //{
            //    Directory.CreateDirectory(directoryPath);
            //}

            var imagePath = Path.Combine(_imageDirectory, fileName);
            var imageData = Convert.FromBase64String(base64Image.Split(',')[1]);

            await System.IO.File.WriteAllBytesAsync(imagePath, imageData);

            return Path.Combine(fileName).Replace("\\", "/");
        }


        [HttpPut("{roomDetailsId}")]
        public async Task<IActionResult> UpdateRoomDetails(int roomDetailsId, [FromBody] RoomDetailsPutRequestDto roomDetailsRequest)
        {
            if (roomDetailsRequest == null || roomDetailsId <= 0)
            {
                return BadRequest(new { status = 400, message = "Invalid request data." });
            }

            var existingRoomDetails = await _context.roomDetailsModels.FindAsync(roomDetailsId);

            if (existingRoomDetails == null)
            {
                return NotFound(new { status = 404, message = "Room details not found." });
            }

            if (roomDetailsRequest.updatedBy.HasValue && !await UserExistsAsync(roomDetailsRequest.updatedBy.Value))
            {
                return NotFound(new { status = 404, message = "User not found." });
            }

            var buildingName = await GetNameByIdAsync(existingRoomDetails.buildingId, "building");
            var floorName = await GetNameByIdAsync(existingRoomDetails.floorId, "floor");
            var roomName = await GetNameByIdAsync(existingRoomDetails.roomId, "room");

            if (string.IsNullOrEmpty(buildingName) || string.IsNullOrEmpty(floorName) || string.IsNullOrEmpty(roomName))
            {
                return NotFound(new { status = 404, message = "Invalid building, floor, or room data." });
            }

            // Process images
            var updatedImages = await ProcessImagesForUpdateAsync(
                roomDetailsRequest.roomImages,
                buildingName,
                floorName,
                roomName,
                roomDetailsRequest.updatedBy ?? 0
            );

            // Update room details
            existingRoomDetails.roomDimension = roomDetailsRequest.roomDimension;
            existingRoomDetails.roomSideId = roomDetailsRequest.roomSideId;
            existingRoomDetails.bedSpecificationId = roomDetailsRequest.bedSpecificationId;
            existingRoomDetails.roomBelconiId = roomDetailsRequest.roomBelconiId;
            existingRoomDetails.attachedBathroomId = roomDetailsRequest.attachedBathroomId;
            existingRoomDetails.commonFeatures = roomDetailsRequest.commonFeatures;
            existingRoomDetails.availableFurnitures = roomDetailsRequest.availableFurnitures;
            //existingRoomDetails.bedSpecification = roomDetailsRequest.bedSpecification;
            existingRoomDetails.bathroomSpecification = roomDetailsRequest.bathroomSpecification;
            existingRoomDetails.roomImages = updatedImages;
            existingRoomDetails.isActive = roomDetailsRequest.isActive ?? existingRoomDetails.isActive;
            existingRoomDetails.updatedBy = roomDetailsRequest.updatedBy ?? existingRoomDetails.updatedBy;
            existingRoomDetails.updatedTime = DateTime.UtcNow;

            _context.roomDetailsModels.Update(existingRoomDetails);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Room details updated successfully!",
                roomDetailsId = existingRoomDetails.roomDetailsId
            });
        }

        // Process images for update
        private async Task<List<string>> ProcessImagesForUpdateAsync(List<string>? roomImages, string buildingName, string floorName, string roomName, int updatedBy)
        {
            var updatedImages = new List<string>();

            if (roomImages != null && roomImages.Any())
            {
                foreach (var image in roomImages)
                {
                    if (image.StartsWith("data:image")) // Base64 image
                    {
                        var imageUrl = await SaveImageAsync(image, buildingName, floorName, roomName, updatedBy);
                        updatedImages.Add(imageUrl);
                    }
                    else // Existing image URL
                    {
                        updatedImages.Add(image);
                    }
                }
            }

            return updatedImages;
        }



        // GET api/admin/room/roomdetails

        //[HttpGet("all")]
        //public async Task<IActionResult> GetRoomDetails()
        //{
        //    try
        //    {
        //        // Fetch all room details that are active
        //        var roomDetailsList = await _context.roomDetailsModels
        //            .Where(r => r.isActive ==true)
        //            .ToListAsync();

        //        if (!roomDetailsList.Any())
        //        {
        //            return NotFound(new { status = 404, message = "No room details found." });
        //        }

        //        // Collect IDs for related data
        //        var roomIds = roomDetailsList.Select(r => r.roomId).Distinct().ToList();
        //        var floorIds = roomDetailsList.Select(r => r.floorId).Distinct().ToList();
        //        var buildingIds = roomDetailsList.Select(r => r.buildingId).Distinct().ToList();
        //        var commonFeatureIds = roomDetailsList.SelectMany(r => r.commonFeatures).Distinct().ToList();
        //        var availableFurnitureIds = roomDetailsList.SelectMany(r => r.availableFurnitures).Distinct().ToList();
        //        var bathroomIds = roomDetailsList.SelectMany(r => r.bathroomSpecification).Distinct().ToList();

        //        // Fetch related data
        //        var rooms = await _context.roomInfoModels
        //            .Where(r => roomIds.Contains(r.roomId))
        //            .ToDictionaryAsync(r => r.roomId, r => r.roomName);

        //        var floors = await _context.floorInfoModels
        //            .Where(f => floorIds.Contains(f.floorId))
        //            .ToDictionaryAsync(f => f.floorId, f => f.floorName);

        //        var buildings = await _context.buildingInfoModels
        //            .Where(b => buildingIds.Contains(b.buildingId))
        //            .ToDictionaryAsync(b => b.buildingId, b => b.buildingName);

        //        var commonFeatures = await _context.roomCommonFeaturesModels
        //            .Where(cf => commonFeatureIds.Contains(cf.commonFeatureId))
        //            .ToDictionaryAsync(cf => cf.commonFeatureId, cf => cf.name);

        //        var availableFurnitures = await _context.roomAFModels
        //            .Where(af => availableFurnitureIds.Contains(af.availableFurnitureId))
        //            .ToDictionaryAsync(af => af.availableFurnitureId, af => af.name);

        //        var bathroomSpecifications = await _context.roomBathroomModels
        //            .Where(bm => bathroomIds.Contains(bm.bathroomId))
        //            .ToDictionaryAsync(bm => bm.bathroomId, bm => bm.name);

        //        // Construct the response
        //        var response = roomDetailsList.Select(r => new
        //        {
        //            r.roomDetailsId,
        //            r.roomId,
        //            roomName = rooms.TryGetValue(r.roomId, out var roomName) ? roomName : "Unknown",
        //            r.floorId,
        //            floorName = floors.TryGetValue(r.floorId, out var floorName) ? floorName : "Unknown",
        //            r.buildingId,
        //            buildingName = buildings.TryGetValue(r.buildingId, out var buildingName) ? buildingName : "Unknown",
        //            r.roomDimension,
        //            r.roomSideId,
        //            r.bedSpecificationId,
        //            r.roomBelconiId,
        //            r.attachedBathroomId,
        //            commonFeatures = r.commonFeatures?
        //                .Where(cfId => commonFeatures.ContainsKey(cfId)) // Skip invalid IDs
        //                .Select(cfId => new
        //                {
        //                    commonFeatureId = cfId,
        //                    name = commonFeatures[cfId]
        //                }).ToList() ?? Enumerable.Empty<object>().Select(cf => new { commonFeatureId = 0, name = "" }).ToList(),

        //            availableFurnitures = r.availableFurnitures?
        //                .Where(afId => availableFurnitures.ContainsKey(afId)) // Skip invalid IDs
        //                .Select(afId => new
        //                {
        //                    availableFurnitureId = afId,
        //                    name = availableFurnitures[afId]
        //                }).ToList() ?? Enumerable.Empty<object>(),

        //            bathroomSpecification = r.bathroomSpecification?   // Here got an error as mentioned below
        //                .Where(bathId => bathroomSpecifications.ContainsKey(bathId)) // Skip invalid IDs
        //                .Select(bathId => new
        //                {
        //                    bathroomId = bathId,
        //                    name = bathroomSpecifications[bathId]
        //                }).ToList() ?? Enumerable.Empty<object>(),
        //            roomImages = r.roomImages?
        //                .Where(image => !string.IsNullOrEmpty(image)) // Ignore null or empty images
        //                .Select(image => $"images/{image}") // Format image URLs
        //                .ToList() ?? new List<string>(),
        //            r.isApprove,
        //            r.approvedBy,
        //            r.isActive,
        //            r.inactiveBy,
        //            r.inactiveDate,
        //            r.createdBy,
        //            r.createdTime,
        //            r.updatedBy,
        //            r.updatedTime
        //        }).ToList();

        //        return Ok(new
        //        {
        //            status = 200,
        //            message = "Room details retrieved successfully.",
        //            data = response
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            status = 500,
        //            message = "An error occurred while retrieving room details.",
        //            error = ex.Message
        //        });
        //    }
        //}

        [HttpGet("all")]
        public async Task<IActionResult> GetRoomDetails()
        {
            // Fetch all room details that are active
            var roomDetailsList = await _context.roomDetailsModels
                .Where(r => r.isActive == true)
                .ToListAsync();

            if (!roomDetailsList.Any())
            {
                return NotFound(new { status = 404, message = "No room details found." });
            }

            // Collect IDs for related data
            var roomIds = roomDetailsList.Select(r => r.roomId).Distinct().ToList();
            var floorIds = roomDetailsList.Select(r => r.floorId).Distinct().ToList();
            var buildingIds = roomDetailsList.Select(r => r.buildingId).Distinct().ToList();
            var commonFeatureIds = roomDetailsList.SelectMany(r => r.commonFeatures).Distinct().ToList();
            var availableFurnitureIds = roomDetailsList.SelectMany(r => r.availableFurnitures).Distinct().ToList();
            //var bedIds = roomDetailsList.SelectMany(r => r.bedSpecification).Distinct().ToList();
            var bathroomIds = roomDetailsList.SelectMany(r => r.bathroomSpecification).Distinct().ToList();

            // Fetch related data
            var rooms = await _context.roomInfoModels
                .Where(r => roomIds.Contains(r.roomId))
                .ToDictionaryAsync(r => r.roomId, r => r.roomName);

            var floors = await _context.floorInfoModels
                .Where(f => floorIds.Contains(f.floorId))
                .ToDictionaryAsync(f => f.floorId, f => f.floorName);

            var buildings = await _context.buildingInfoModels
                .Where(b => buildingIds.Contains(b.buildingId))
                .ToDictionaryAsync(b => b.buildingId, b => b.buildingName);

            var commonFeatures = await _context.roomCommonFeaturesModels
                .Where(cf => commonFeatureIds.Contains(cf.commonFeatureId))
                .ToDictionaryAsync(cf => cf.commonFeatureId, cf => cf.name);

            var availableFurnitures = await _context.roomAFModels
                .Where(af => availableFurnitureIds.Contains(af.availableFurnitureId))
                .ToDictionaryAsync(af => af.availableFurnitureId, af => af.name);

            //var bedSpecifications = await _context.roomBedModels
            //    .Where(bs => bedIds.Contains(bs.bedId))
            //    .ToDictionaryAsync(bs => bs.bedId, bs => bs.name);

            var bathroomSpecifications = await _context.roomBathroomModels
                .Where(bm => bathroomIds.Contains(bm.bathroomId))
                .ToDictionaryAsync(bm => bm.bathroomId, bm => bm.name);

            // Construct the response
            var response = roomDetailsList.Select(r => new
            {
                r.roomDetailsId,
                r.roomId,
                roomName = rooms.TryGetValue(r.roomId, out var roomName) ? roomName : "Unknown",
                r.floorId,
                floorName = floors.TryGetValue(r.floorId, out var floorName) ? floorName : "Unknown",
                r.buildingId,
                buildingName = buildings.TryGetValue(r.buildingId, out var buildingName) ? buildingName : "Unknown",
                r.roomDimension,
                r.roomSideId,
                r.bedSpecificationId,
                r.roomBelconiId,
                r.attachedBathroomId,
                commonFeatures = r.commonFeatures
                    .Select(cfId => new
                    {
                        commonFeatureId = cfId,
                        name = commonFeatures.TryGetValue(cfId, out var cfName) ? cfName : "Unknown"
                    }).ToList(),
                availableFurnitures = r.availableFurnitures
                    .Select(afId => new
                    {
                        availableFurnitureId = afId,
                        name = availableFurnitures.TryGetValue(afId, out var afName) ? afName : "Unknown"
                    }).ToList(),
                //bedSpecification = r.bedSpecification
                //    .Select(bedId => new
                //    {
                //        bedId = bedId,
                //        name = bedSpecifications.TryGetValue(bedId, out var bedName) ? bedName : "Unknown"
                //    }).ToList(),
                bathroomSpecification = r.bathroomSpecification
                    .Select(bathId => new
                    {
                        bathroomId = bathId,
                        name = bathroomSpecifications.TryGetValue(bathId, out var bathName) ? bathName : "Unknown"
                    }).ToList(),
                //roomImages = r.roomImages  // Here ignore the null value 
                //    .Select(image => $"images/{image}") // Format roomImages URLs
                //    .ToList(),
                roomImages = r.roomImages?
                    .Where(image => !string.IsNullOrEmpty(image)) // Ignore null or empty values
                    .Select(image => $"images/{image}") // Format roomImages URLs
                    .ToList() ?? new List<string>(),

                r.isApprove,
                r.approvedBy,
                r.isActive,
                r.inactiveBy,
                r.inactiveDate,
                r.createdBy,
                r.createdTime,
                r.updatedBy,
                r.updatedTime
            }).ToList();

            return Ok(new
            {
                status = 200,
                message = "Room details retrieved successfully.",
                data = response
            });
        }



        [HttpGet]
        public async Task<IActionResult> GetRoomDetails(
         [FromQuery] int userId,
         [FromQuery] int buildingId,
         [FromQuery] int floorId,
         [FromQuery] int roomId)
        {
            // Validate user existence
            if (!await UserExistsAsync(userId))
            {
                return NotFound(new { status = 404, message = "User not found." });
            }

            // Fetch room details
            var roomDetails = await _context.roomDetailsModels
                .Where(r => r.isActive == true &&
                            r.buildingId == buildingId &&
                            r.floorId == floorId &&
                            r.roomId == roomId)
                .ToListAsync();

            if (!roomDetails.Any())
            {
                return NotFound(new { status = 404, message = "No room details found for the specified criteria." });
            }

            // Fetch related data (commonFeatures, availableFurnitures, bedSpecifications, bathroomSpecifications)
            var commonFeatureIds = roomDetails.SelectMany(r => r.commonFeatures).Distinct().ToList();
            var availableFurnitureIds = roomDetails.SelectMany(r => r.availableFurnitures).Distinct().ToList();
            //var bedIds = roomDetails.SelectMany(r => r.bedSpecification).Distinct().ToList();
            var bathroomIds = roomDetails.SelectMany(r => r.bathroomSpecification).Distinct().ToList();

            var commonFeatures = await _context.roomCommonFeaturesModels
                .Where(cf => commonFeatureIds.Contains(cf.commonFeatureId))
                .ToDictionaryAsync(cf => cf.commonFeatureId);

            var availableFurnitures = await _context.roomAFModels
                .Where(af => availableFurnitureIds.Contains(af.availableFurnitureId))
                .ToDictionaryAsync(af => af.availableFurnitureId);

            //var bedSpecifications = await _context.roomBedModels
            //    .Where(bs => bedIds.Contains(bs.bedId))
            //    .ToDictionaryAsync(bs => bs.bedId);

            var bathroomSpecifications = await _context.roomBathroomModels
                .Where(bm => bathroomIds.Contains(bm.bathroomId))
                .ToDictionaryAsync(bm => bm.bathroomId);

            // Fetch room, floor, and building names
            var roomInfo = await _context.roomInfoModels.FirstOrDefaultAsync(r => r.roomId == roomId);
            var floorInfo = await _context.floorInfoModels.FirstOrDefaultAsync(f => f.floorId == floorId);
            var buildingInfo = await _context.buildingInfoModels.FirstOrDefaultAsync(b => b.buildingId == buildingId);

            var roomName = roomInfo?.roomName ?? "Unknown";
            var floorName = floorInfo?.floorName ?? "Unknown";
            var buildingName = buildingInfo?.buildingName ?? "Unknown";

            // Construct response
            var response = roomDetails.Select(r => new
            {
                r.roomDetailsId,
                r.roomId,
                roomName, // Include room name
                r.floorId,
                floorName, // Include floor name
                r.buildingId,
                buildingName, // Include building name
                r.roomDimension,
                r.roomSideId,
                r.bedSpecificationId,
                r.roomBelconiId,
                r.attachedBathroomId,
                commonFeatures = r.commonFeatures
                    .Select(cfId => new
                    {
                        commonFeaturesId = cfId,
                        name = commonFeatures.TryGetValue(cfId, out var cf) ? cf.name : "Unknown"
                    }).ToList(),
                availableFurnitures = r.availableFurnitures
                    .Select(afId => new
                    {
                        availableFurnitureId = afId,
                        name = availableFurnitures.TryGetValue(afId, out var af) ? af.name : "Unknown"
                    }).ToList(),
                //bedSpecification = r.bedSpecification
                //    .Select(bedId => new
                //    {
                //        bedId = bedId,
                //        name = bedSpecifications.TryGetValue(bedId, out var bed) ? bed.name : "Unknown"
                //    }).ToList(),
                bathroomSpecification = r.bathroomSpecification
                    .Select(bathId => new
                    {
                        bathroomId = bathId,
                        name = bathroomSpecifications.TryGetValue(bathId, out var bm) ? bm.name : "Unknown"
                    }).ToList(),
                roomImages = r.roomImages
                    .Select(image => $"images/{image}") // Format roomImages URLs
                    .ToList(),
                r.isApprove,
                r.approvedBy,
                r.isActive,
                r.inactiveBy,
                r.inactiveDate,
                r.createdBy,
                r.createdTime,
                r.updatedBy,
                r.updatedTime
            }).ToList();

            return Ok(new
            {
                status = 200,
                message = "Room details retrieved successfully.",
                data = response
            });
        }


        // DELETE api/admin/room/roomdetails/
        [HttpDelete("{roomDetailsId:int}")]
        public async Task<IActionResult> DeleteRoomDetails(int roomDetailsId, [FromBody] RoomDetailsDeleteRequestDto requestDto)
        {
            // Validate if user exists
            if (!await UserExistsAsync(requestDto.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Bed
            var deletedRoomDetails= await _context.roomDetailsModels.FindAsync(roomDetailsId);

            if (deletedRoomDetails == null || deletedRoomDetails.isActive == false)
            {
                return NotFound(new { status = 404, message = "Room Details not found or already inactive" });
            }

            // Perform soft delete
            deletedRoomDetails.isActive = false;
            deletedRoomDetails.inactiveBy = requestDto.inactiveBy;
            deletedRoomDetails.inactiveDate = DateTime.UtcNow;

            _context.roomDetailsModels.Update(deletedRoomDetails);

            // Update the haveRoomDetails property in RoomInfoModel
            var roomInfo = await _context.roomInfoModels
                .FirstOrDefaultAsync(r => r.roomId == deletedRoomDetails.roomId &&
                                          r.floorId == deletedRoomDetails.floorId &&
                                          r.buildingId == deletedRoomDetails.buildingId &&
                                          r.isActive == true);

            if (roomInfo == null)
            {
                return NotFound(new { status = 404, message = "Room not found for updating haveRoomDetails properties." });
            }

            roomInfo.haveRoomDetails = false;
            _context.roomInfoModels.Update(roomInfo);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Room Details deleted successfully (soft delete)",
                data = deletedRoomDetails
            });
        }


    }
}


