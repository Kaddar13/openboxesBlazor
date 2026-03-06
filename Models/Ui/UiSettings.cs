namespace OpenBoxesMobile.Blazor.Models.Ui;

public sealed class UiSettings
{
    public string ServerUrl { get; set; } = string.Empty;
    public bool GroupLocationEntries { get; set; }
    public int BarcodeScanDebounceTime { get; set; } = 100;
    public Dictionary<string, bool> DashboardEntriesVisibility { get; set; } = new();
    public List<string> DashboardEntriesOrder { get; set; } = new();
}
