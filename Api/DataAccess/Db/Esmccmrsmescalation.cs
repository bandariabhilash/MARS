using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Esmccmrsmescalation
{
    public int Id { get; set; }

    public string? Zipcode { get; set; }

    public string? Country { get; set; }

    public double? Edsmid { get; set; }

    public string? Esmname { get; set; }

    public string? Esmphone { get; set; }

    public string? Esmemail { get; set; }

    public double? Ccmid { get; set; }

    public string? Ccmname { get; set; }

    public string? Ccmemail { get; set; }

    public string? Ccmphone { get; set; }

    public double? Rsmid { get; set; }

    public string? Rsm { get; set; }

    public string? Rsmemail { get; set; }

    public string? Rsmphone { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
