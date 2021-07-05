using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class BaseModel
    {
        public string ACCESS_READ_ONLY = "read-only";        
        public string ACCESS_EDIT_ONLY = "edit-only";
        public string ACCESS_FULL = "full";
        public string ACCESS_NO_PERMISSION = "no-permission";

    }
}