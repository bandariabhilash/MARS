namespace ServiceApis.Models
{
    public class ERFRequestModel
    {
        public int AccountNumber { get; set; }
        public string MainContactNum { get; set; }
        public string MainContactName { get; set; }
        public string ErfNotes { get; set; }
        public bool CreateWorkorder { get; set; }
        public string OrderType { get; set; }
        public string ShipToBranch { get; set; }
        public string InstallDate { get; set; }
        public decimal AdditionalNSV { get; set; }
        public string HoursofOperation { get; set; }
        public string InstallLocation { get; set; }
        public string SiteReady { get; set; }
        public DateTime FormDate { get; set; }
        public DateTime ERFReceivedDate { get; set; }
        public DateTime ERFProcessedDate { get; set; }

        public IList<ERFEquipmentModel> EquipmentData { get; set; } = new List<ERFEquipmentModel>();
        public IList<ERFExpendableModel> ExpendableData { get; set; } = new List<ERFExpendableModel>();
    }

    public class ERFEquipmentModel
    {
        public string EqpCategory { get; set; }
        public string EqpBrand { get; set; }
        public int EqpQuantity { get; set; }
        public string EqpUsingBranch { get; set; }
        public string EqpSubstitutionPossible { get; set; }
        public string EqpTransType { get; set; }
        public string EqpType { get; set; }        
    }
    public class ERFExpendableModel
    {
        public string ExpCategory { get; set; }
        public string ExpBrand { get; set; }
        public int ExpQuantity { get; set; }
        public string ExpUsingBranch { get; set; }
        public string ExpSubstitutionPossible { get; set; }
        public string ExpTransType { get; set; }
        public string ExpType { get; set; }
    }
}
