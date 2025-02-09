namespace snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto
{
    public class putPaidItemDto
    {
        public required string name { get; set; }
        public required string price { get; set; }
        public required int paidOrFree { get; set; }
        public required string remarks { get; set; }
        public required int updatedBy { get; set; }
        public DateTime? updatedTime { get; set; }
    }
}
