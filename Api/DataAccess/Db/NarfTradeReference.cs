using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NarfTradeReference
{
    public int TradeRefId { get; set; }

    public int? ApplicationId { get; set; }

    public string? CompanyName { get; set; }

    public string? AccountId { get; set; }

    public string? Phone { get; set; }

    public int CreatedUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int ModifiedUserId { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual NarfNewAccount? Application { get; set; }
}
