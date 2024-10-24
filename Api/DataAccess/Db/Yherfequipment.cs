using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Yherfequipment
{
    public string Erfno { get; set; } = null!;

    public int Sno { get; set; }

    public int? EquipQuantity { get; set; }

    public string? EquipProdNo { get; set; }

    public string? EquipType { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? TransactionTypeId { get; set; }

    public string? EquipModelNo { get; set; }

    public string? Substitution { get; set; }

    public string? EquipDescription { get; set; }

    public decimal? Extra { get; set; }

    public decimal? TotalPrice { get; set; }
}
