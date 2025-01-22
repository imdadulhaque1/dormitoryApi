using System.ComponentModel.DataAnnotations;

namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.roomDetails
{
    public class RoomDetailsPostRequestDto
    {
        public int roomId { get; set; }
        public int floorId { get; set; }
        public int buildingId { get; set; }
        public required string roomDimension { get; set; }

        // Room side (1=east, 2=west, 3=north, 4=south)
        public required int roomSideId { get; set; }

        // 1=Attached Belconi, 2=No Belconi
        public required int roomBelconiId { get; set; }

        // 1=yes, 2=no
        public required int attachedBathroomId { get; set; }
        public required List<int> commonFeatures { get; set; }
        public required List<int> availableFurnitures { get; set; }
        public required List<int> bedSpecification { get; set; }
        public required List<int> bathroomSpecification { get; set; }

        // Room image URLs
        public List<string>? roomImages { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
