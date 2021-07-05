using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public enum TimeZoneEnum
    {
        E = -5,
        C = -6,
        M = -7,
        P = -8,
        K = -9
    };

    public enum Roles
    {
        Administration = 101,
        StandardUser = 102,
        ProjectOwner= 103       
    }

    public enum Access
    {
        No = 0,
        Yes = 1,
        NotApplicable = -1
    }
}