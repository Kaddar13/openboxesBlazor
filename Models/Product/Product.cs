namespace OpenBoxesMobile.Blazor.Models.Product;

public sealed class Product
{
    public string Id { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
