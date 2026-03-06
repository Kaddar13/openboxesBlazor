namespace OpenBoxesMobile.Blazor.Models.Ui;

public sealed record DashboardEntryDefinition(
    string Key,
    string ScreenName,
    string Description,
    string? Route,
    bool DefaultVisible = true
);
