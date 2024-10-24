using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ServiceApis.Models
{
    public class WorkorderRequestModel
    {
        [DefaultValue(0)]
        public int AccountNumber { get; set; }
        [DefaultValue(0)]
        public int ERFId { get; set; }
        [MaxLength(150, ErrorMessage = "Customer Name cannot be longer than 150 characters.")]
        [DefaultValue("")]
        public string CustomerName { get; set; }
        [DefaultValue("")]
        public string Address1 { get; set; }
        [DefaultValue("")]
        public string Address2 { get; set; }
        [DefaultValue("")]
        public string City { get; set; }
        [MaxLength(2, ErrorMessage = "State/Province should be 2 character Code.")]
        [DefaultValue("")]
        public string State { get; set; }
        
        [DefaultValue("")]
        public string PostalCode { get; set; }
        [DefaultValue("")]
        public string MainContactNum { get; set; }
        [DefaultValue("")]
        public string MainContactName { get; set; }
        [DefaultValue("")]
        public string Comments { get; set; }
    }
}
