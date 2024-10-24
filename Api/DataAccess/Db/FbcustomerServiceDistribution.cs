using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbcustomerServiceDistribution
{
    public int ServiceId { get; set; }

    public string? Route { get; set; }

    public string? Branch { get; set; }

    public string? Rsrname { get; set; }

    public string? Rsrphone { get; set; }

    public string? Rsremail { get; set; }

    public string? SalesManagerName { get; set; }

    public string? SalesManagerPhone { get; set; }

    public string? SalesMmanagerEmail { get; set; }

    public string? RegionalsName { get; set; }

    public string? RegonalsPhone { get; set; }

    public string? RegionalsEmail { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? CreatedUserId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
