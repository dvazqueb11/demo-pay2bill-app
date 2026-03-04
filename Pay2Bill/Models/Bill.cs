namespace Pay2Bill.Models
{
    public enum BillType
    {
        Internet,
        Phone
    }

    public class Bill
    {
        public int Id { get; set; }
        public BillType Type { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal AmountDue { get; set; }
        public System.DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public string Description { get; set; }

        public string TypeDisplayName => Type == BillType.Internet ? "Internet" : "Phone";
        public string TypeIcon => Type == BillType.Internet ? "wifi" : "phone";
    }
}
