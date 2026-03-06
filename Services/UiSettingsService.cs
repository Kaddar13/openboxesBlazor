using System.Text.Json;
using Microsoft.JSInterop;
using OpenBoxesMobile.Blazor.Models.Ui;

namespace OpenBoxesMobile.Blazor.Services;

public sealed class UiSettingsService
{
    private const string StorageKey = "openboxes.ui.settings";
    private readonly IJSRuntime _jsRuntime;
    private readonly OpenBoxesApiClient _apiClient;

    public UiSettings Current { get; private set; } = new();
    public bool Loaded { get; private set; }

    public event Action? Changed;

    public UiSettingsService(IJSRuntime jsRuntime, OpenBoxesApiClient apiClient)
    {
        _jsRuntime = jsRuntime;
        _apiClient = apiClient;
    }

    public async Task EnsureLoadedAsync(string defaultServerUrl)
    {
        if (Loaded)
        {
            return;
        }

        try
        {
            var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var parsed = JsonSerializer.Deserialize<UiSettings>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsed is not null)
                {
                    Current = parsed;
                }
            }
        }
        catch
        {
            // Ignore storage access failures and keep defaults.
        }

        if (string.IsNullOrWhiteSpace(Current.ServerUrl) && !string.IsNullOrWhiteSpace(defaultServerUrl))
        {
            Current.ServerUrl = defaultServerUrl;
        }

        if (!string.IsNullOrWhiteSpace(Current.ServerUrl))
        {
            _apiClient.SetBaseUrl(Current.ServerUrl);
        }

        Loaded = true;
        Notify();
    }

    public async Task SaveAsync()
    {
        var raw = JsonSerializer.Serialize(Current);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, raw);
        Notify();
    }

    public void ResetDashboard()
    {
        Current.DashboardEntriesOrder = [];
        Current.DashboardEntriesVisibility = new Dictionary<string, bool>();
        Notify();
    }

    private void Notify() => Changed?.Invoke();
}
