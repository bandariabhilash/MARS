using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class YhsmeventSymcall
{
    public int EventId { get; set; }

    public int AssetKey { get; set; }

    public int? CallTypeId { get; set; }

    public string? ManufacturerDesc { get; set; }

    public string? ProductNo { get; set; }

    public string? SerialNo { get; set; }

    public int? SolutionId { get; set; }

    public string? Temperature { get; set; }

    public string? Settings { get; set; }

    public int? SystemId { get; set; }

    public int? SymptomId { get; set; }

    public string? CategoryDesc { get; set; }

    public string? Manufacturer { get; set; }

    public string? EquipTypeCode { get; set; }

    public string? Electricity { get; set; }

    public string? Plumbing { get; set; }

    public string? NeworReman { get; set; }

    public string? Defective { get; set; }

    public string? Category { get; set; }
}
