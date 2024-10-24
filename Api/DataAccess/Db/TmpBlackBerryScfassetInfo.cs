using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpBlackBerryScfassetInfo
{
    public int UniqueId { get; set; }

    public int? WorkorderId { get; set; }

    public int? TechId { get; set; }

    public int? ServiceCode { get; set; }

    public int? CompletionCode { get; set; }

    public string? EquipTypeCode { get; set; }

    public string? VendorCode { get; set; }

    public string? Model { get; set; }

    public string? SerialNo { get; set; }

    public DateTime? EntryDate { get; set; }

    public short? CloseCall { get; set; }

    public short? EventUpdated { get; set; }

    public int? SpawnEventId { get; set; }

    public int? AssetKey { get; set; }

    public string? ManufacturerDesc { get; set; }

    public string? CategoryDesc { get; set; }

    public string? ClosureConfirmationNo { get; set; }

    public string? Notes { get; set; }

    public short? Sent5191Email { get; set; }

    public string? TechName { get; set; }

    public string? Weight { get; set; }

    public string? Ratio { get; set; }

    public short? SpawnOrderCreated { get; set; }

    public int? SymptomCode { get; set; }

    public int? SystemCode { get; set; }

    public string? Temperature { get; set; }

    public string? ReasonCode { get; set; }
}
