namespace OpenBoxesMobile.Blazor.Models.Picking;

public sealed class PickTask
{
    public string Id { get; set; } = string.Empty;
    public string? Identifier { get; set; }
    public string? Status { get; set; }
    public string? RequisitionNumber { get; set; }
    public string? DeliveryTypeCode { get; set; }
    public int QuantityRequired { get; set; }
    public int QuantityPicked { get; set; }
    public DateTime? DateCreated { get; set; }
}
