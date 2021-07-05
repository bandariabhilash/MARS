

namespace FarmerBrothers.Models
{
    public class WorkOrderDetailsModel
    {
        public string StartDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string CompletionDateTime { get; set; }
        public string TechnicianName { get; set; }
        public string MilageToCustomer { get; set; }
        public string TravelToCustomerTimeHours { get; set; }
        public string TravelToCustomerTimeMinutes { get; set; }
        public string PhoneSolve { get; set; }
        public string SpecialClosure { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public byte[] CustomerSignature { get; set; }
        public string InvoiceNo { get; set; }
    }
}