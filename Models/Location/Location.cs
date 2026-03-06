namespace OpenBoxesMobile.Blazor.Models.Location;

public sealed class Location
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationGroup? LocationGroup { get; set; }
    public string? OrganizationName { get; set; }
    public bool HasBinLocationSupport { get; set; }
    public bool HasPackingSupport { get; set; }
    public bool HasPartialReceivingSupport { get; set; }
    public LocationType? LocationType { get; set; }
    public string? LocationNumber { get; set; }
    public bool IsDisplay { get; set; }
}
