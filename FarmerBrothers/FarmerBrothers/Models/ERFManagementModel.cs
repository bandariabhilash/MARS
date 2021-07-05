using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public enum ERFManagementSubmitType
    {
        NONE = 0,
        SAVE = 1,
        CREATEERF = 2,
        CREATEONLYERF = 3,
        CREATEERFWITHWORKORDER=4
    }
    public class ERFManagementModel
    {
        public Contact Customer;
        public NotesModel Notes;
        public Erf Erf;

        public IList<ERFManagementExpendableModel> NonSerializedList;
        public IList<ERFManagementEquipmentModel> SerializedList;

    }
}