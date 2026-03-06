using OpenBoxesMobile.Blazor.Models.Auth;

namespace OpenBoxesMobile.Blazor.Services;

public sealed class SessionManager
{
    private readonly OpenBoxesApiClient _apiClient;
    private readonly AppState _appState;
    private bool _sessionLoadAttempted;

    public SessionManager(OpenBoxesApiClient apiClient, AppState appState)
    {
        _apiClient = apiClient;
        _appState = appState;
    }

    public async Task<Session?> EnsureSessionAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && _appState.IsAuthenticated)
        {
            return _appState.CurrentSession;
        }

        if (!forceRefresh && _sessionLoadAttempted)
        {
            return _appState.CurrentSession;
        }

        _sessionLoadAttempted = true;

        try
        {
            var session = await _apiClient.GetSessionAsync(cancellationToken);
            _appState.SetSession(session);
            _appState.ClearError();
            return session;
        }
        catch (Exception ex)
        {
            _appState.SetSession(null);
            _appState.SetError($"Session error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.LoginAsync(username, password, cancellationToken);
            var session = await _apiClient.GetSessionAsync(cancellationToken);
            _appState.SetSession(session);
            _appState.ClearError();
            _sessionLoadAttempted = true;
            return _appState.IsAuthenticated;
        }
        catch (Exception ex)
        {
            _appState.SetError($"Login failed: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.LogoutAsync(cancellationToken);
        }
        catch
        {
            // Ignore API logout failures and clear local state.
        }

        _sessionLoadAttempted = true;
        _appState.SetSession(null);
        _appState.ClearError();
    }
}
