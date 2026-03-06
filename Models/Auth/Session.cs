using OpenBoxesMobile.Blazor.Models.Common;

namespace OpenBoxesMobile.Blazor.Models.Auth;

public sealed class Session
{
    public OpenBoxesMobile.Blazor.Models.Location.Location? Location { get; set; }
    public bool IsSuperuser { get; set; }
    public bool IsUserAdmin { get; set; }
    public string? ActiveLanguage { get; set; }
    public User? User { get; set; }
    public string? LogoLabel { get; set; }
    public string? LogoUrl { get; set; }
    public List<Locale>? SupportedLocales { get; set; }
}
