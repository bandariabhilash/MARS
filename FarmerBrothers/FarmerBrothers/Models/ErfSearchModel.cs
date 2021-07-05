using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;

namespace FarmerBrothers.Models
{
    public class ErfSearchModel
    {
        public ErfSearchModel()
        {
            CustomerId = string.Empty;
            CustomerName = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ERFID = string.Empty;
            WorkOrderID = string.Empty;
            FeastMovement = string.Empty;
            Reason = string.Empty;
            ZipCode = string.Empty;

            CreatedFrom = new Nullable<DateTime>();
            CreatedTo = new Nullable<DateTime>();

            using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
            {
                States = Utility.GetStates(entities);
            }
        }

        public IList<State> States;
        public IList<ErfSearchResultModel> SearchResults;
        public IList<AllFBStatu> Reasons;

        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ERFID { get; set; }
        public string WorkOrderID { get; set; }
        public string FeastMovement { get; set; }
        public string Reason { get; set; }
        public string ZipCode { get; set; }
        public string OriginatorName { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public List<ESMCCMRSMEscalation> EsmList { get; set; }
        public List<string> Esm { get; set; }
    }
}