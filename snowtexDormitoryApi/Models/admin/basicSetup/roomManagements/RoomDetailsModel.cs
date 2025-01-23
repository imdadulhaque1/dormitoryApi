using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.Models.admin.basicSetup.roomManagements
{
    public class RoomDetailsModel
    {
        [Key]
        public int roomDetailsId { get; set; }

        public int roomId { get; set; }
        public int floorId { get; set; }
        public int buildingId { get; set; }

        // public bool? isAvailable { get; set; } = true;

        // Room dimension (e.g. size or area description)
        public required string roomDimension { get; set; }

        // Room side (1=east, 2=west, 3=north, 4=south)
        public required int roomSideId { get; set; }

        // Single=1, Double=2, Queen=3, King=4
        public int? bedSpecificationId { get; set; }

        // Indicates if room has a balcony (1=Attached Belconi, 2=No Belconi)
        public required int roomBelconiId { get; set; }

        // Indicates if the room has an attached bathroom (1=yes, 2=no)
        public required int attachedBathroomId { get; set; }

        // JSON array of common features available in the room (e.g., amenities)
        public required List<int> commonFeatures { get; set; }

        // JSON array of available furniture IDs in the room
        public required List<int> availableFurnitures { get; set; }

        // JSON array of bed specifications
       // public required List<int> bedSpecification { get; set; }

        // JSON array of bathroom specifications
        public required List<int> bathroomSpecification { get; set; }

        // Room image URLs
        public List<string>? roomImages { get; set; }

        // Approval status (default is false)
        public bool? isApprove { get; set; } = false;

        // The user who approved the room details (nullable)
        public int? approvedBy { get; set; }

        public DateTime? approvedTime { get; set; }
        // Is the room active (default is true)
        public bool? isActive { get; set; } = true;

        // The user who deactivated the room (nullable)
        public int? inactiveBy { get; set; }

        // Date when the room was deactivated (nullable)
        public DateTime? inactiveDate { get; set; }

        // The user who created the room details
        public required int createdBy { get; set; }

        // Date when the room details were created (nullable, defaults to current time)
        public DateTime? createdTime { get; set; }

        // The user who last updated the room details (nullable)
        public int? updatedBy { get; set; }

        // Date when the room details were last updated (nullable)
        public DateTime? updatedTime { get; set; }
    }
}
