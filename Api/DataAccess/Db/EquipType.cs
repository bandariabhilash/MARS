using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class EquipType
{
    public string? EquipTypeCode { get; set; }

    public string? Description { get; set; }

    public int EquipTypeId { get; set; }

    public int? Sequence { get; set; }
}
