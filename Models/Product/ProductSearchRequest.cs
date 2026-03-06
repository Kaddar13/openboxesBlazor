namespace OpenBoxesMobile.Blazor.Models.Product;

public sealed class ProductSearchRequest
{
    public List<SearchAttribute> SearchAttributes { get; set; } = [];

    public sealed class SearchAttribute
    {
        public string Property { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
