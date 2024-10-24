using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NarfNewAccount
{
    public int ApplicationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Dbaname { get; set; }

    public string? BilllingAddress { get; set; }

    public string? BilllingState { get; set; }

    public string? BilllingCity { get; set; }

    public string? BilllingZip { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? DeliveryState { get; set; }

    public string? DeliveryCity { get; set; }

    public string? DeliveryZip { get; set; }

    public string? CompanyType { get; set; }

    public string? PrincipalOfficer { get; set; }

    public string? PrincipalOfficerTitle { get; set; }

    public string? NatureOfBusiness { get; set; }

    public string? EstablishedYear { get; set; }

    public string? StateIncorporated { get; set; }

    public string? FederalTaxId { get; set; }

    public string? ResaleCertificateNumber { get; set; }

    public bool? TaxExempt { get; set; }

    public bool? Porequired { get; set; }

    public bool? AlreadyHasFbaccount { get; set; }

    public string? Fbaccount { get; set; }

    public string? AccountPayableContact { get; set; }

    public string? AccountPayableTitle { get; set; }

    public string? AccountPayablePhone { get; set; }

    public string? AccountPayableEmail { get; set; }

    public decimal? CreditRequested { get; set; }

    public int CreatedUserId { get; set; }

    public string CreatedUserName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int ModifiedUserId { get; set; }

    public string ModifiedUserName { get; set; } = null!;

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<NarfTradeReference> NarfTradeReferences { get; set; } = new List<NarfTradeReference>();
}
