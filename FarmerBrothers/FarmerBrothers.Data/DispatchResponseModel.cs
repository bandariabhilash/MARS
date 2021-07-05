

namespace FarmerBrothers.Data
{
    public class DispatchResponseModel
    {
        public int TechId { get; set; }
        public int WorkOrderId { get; set; }
        public string Message { get; set; }

        public bool IsERF = false;
    }
}