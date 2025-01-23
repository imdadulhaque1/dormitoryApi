namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomManagementsDto.roomDetails
{
    public class RoomDetailsPutRequestDto
    {
        //public int roomId { get; set; }
        //public int floorId { get; set; }
        //public int buildingId { get; set; }
        //public int roomDetailsId { get; set; }
        public required string roomDimension { get; set; }

        // Room side (1=east, 2=west, 3=north, 4=south)
        public required int roomSideId { get; set; }
        
        // Single=1, Double=2, Queen=3, King=4
        public int? bedSpecificationId { get; set; }

        // 1=Attached Belconi, 2=No Belconi
        public required int roomBelconiId { get; set; }

        // 1=yes, 2=no
        public required int attachedBathroomId { get; set; }
        public required List<int> commonFeatures { get; set; }
        public required List<int> availableFurnitures { get; set; }
        //public required List<int> bedSpecification { get; set; }
        public required List<int> bathroomSpecification { get; set; }

        // Room image URLs
        public List<string>? roomImages { get; set; }
        public bool? isActive { get; set; } = true;
        public int? updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
