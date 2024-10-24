using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NonFbcustomer
{
    public int Id { get; set; }

    public string? NonFbcustomerId { get; set; }

    public string? NonFbcustomerName { get; set; }

    public bool? IsActive { get; set; }
}
