namespace snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto
{
    public class postPaidItemDto
    {
        public required string name { get; set; }
        public required string price { get; set; }
        public required int priceCalculateBy { get; set; }
        public required string remarks { get; set; }
        public required int createdBy { get; set; }
        public DateTime? createdTime { get; set; }
    }
}
