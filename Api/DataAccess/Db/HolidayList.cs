using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class HolidayList
{
    public DateTime? HolidayDate { get; set; }

    public string? HolidayName { get; set; }

    public int UniqueId { get; set; }

    public bool Status { get; set; }
}
