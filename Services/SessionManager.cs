using OpenBoxesMobile.Blazor.Models.Auth;

namespace OpenBoxesMobile.Blazor.Services;

public sealed class SessionManager
{
    private readonly OpenBoxesApiClient _apiClient;
    private readonly AppState _appState;
    private readonly DebugLogService _debug;
    private bool _sessionLoadAttempted;

    public SessionManager(OpenBoxesApiClient apiClient, AppState appState, DebugLogService debug)
    {
        _apiClient = apiClient;
        _appState = appState;
        _debug = debug;
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
            _debug.Info("Session", "Fetching current session");
            var session = await _apiClient.GetSessionAsync(cancellationToken);
            _appState.SetSession(session);
            _appState.ClearError();
            _debug.Info("Session", session?.User is null ? "No authenticated user in session" : $"Authenticated user: {session.User.Username}");
            return session;
        }
        catch (Exception ex)
        {
            _appState.SetSession(null);
            _appState.SetError($"Session error: {ex.Message}");
            _debug.Error("Session", $"Session fetch failed: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _debug.Info("Session", $"Login attempt for '{username}'");
            await _apiClient.LoginAsync(username, password, cancellationToken);
            Session? session;
            try
            {
                session = await _apiClient.GetSessionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _debug.Error("Session", $"getAppContext failed after login: {ex.Message}");
                session = null;
            }

            if (session?.User is null)
            {
                // OpenBoxes can return 500 on getAppContext for some user/location states.
                // Keep the authenticated cookie and let the user continue to location selection.
                _debug.Error("Session", "getAppContext unavailable after successful login, applying degraded session fallback.");
                session = new Session
                {
                    User = new User
                    {
                        Username = username
                    }
                };
            }

            _appState.SetSession(session);
            _appState.ClearError();
            _sessionLoadAttempted = true;
            _debug.Info("Session", _appState.IsAuthenticated ? "Login succeeded" : "Login response received but user not authenticated");
            return _appState.IsAuthenticated;
        }
        catch (Exception ex)
        {
            _appState.SetError($"Login failed: {ex.Message}");
            _debug.Error("Session", $"Login failed: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.LogoutAsync(cancellationToken);
            _debug.Info("Session", "Logout API call succeeded");
        }
        catch
        {
            // Ignore API logout failures and clear local state.
            _debug.Error("Session", "Logout API call failed");
        }

        _sessionLoadAttempted = true;
        _appState.SetSession(null);
        _appState.ClearError();
    }
}
