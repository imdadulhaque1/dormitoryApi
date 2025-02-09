//using snowtexDormitoryApi.Models.admin.basicSetup;

//namespace snowtexDormitoryApi.DTOs.admin.roomBooking
//{
//    public class ItemDto
//    {
//        public required int itemId { get; set; }
//        public required string name { get; set; }
//        public required string price { get; set; }
//        public required int paidOrFree { get; set; }
//        public string? remarks { get; set; }
//        public bool isApprove { get; set; }
//        public int? approvedBy { get; set; }
//        public bool isActive { get; set; }
//        public int? inactiveBy { get; set; }
//        public DateTime? inactiveTime { get; set; }
//        public required int createdBy { get; set; }
//        public DateTime createdTime { get; set; }
//        public int? updatedBy { get; set; }
//        public DateTime? updatedTime { get; set; }
//    }

//    public class postRoomBookingDto
//    {
//        public required int roomId { get; set; }
//        public required int personId { get; set; }
//        public List<PaidItemsModels>? paidItems { get; set; }  // Fix here  
//        public List<PaidItemsModels>? freeItems { get; set; }  // Fix here
//        public required DateTime startTime { get; set; }
//        public required DateTime endTime { get; set; }
//        public string? remarks { get; set; }
//        public required int createdBy { get; set; }
//        public DateTime? createdTime { get; set; }
//    }
//}





namespace snowtexDormitoryApi.DTOs.admin.roomBooking
{
    public class postRoomBookingDto
    {
        public required int roomId { get; set; }
        public required int personId { get; set; }
        public string paidItems { get; set; }
        public string? freeItems { get; set; }
        public required DateTime startTime { get; set; }
        public required DateTime endTime { get; set; }
        public string? remarks { get; set; }

        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
