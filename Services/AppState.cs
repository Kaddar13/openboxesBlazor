using OpenBoxesMobile.Blazor.Models.Auth;

namespace OpenBoxesMobile.Blazor.Services;

public sealed class AppState
{
    public Session? CurrentSession { get; private set; }
    public string? LastError { get; private set; }

    public bool IsAuthenticated => CurrentSession?.User is not null;

    public event Action? StateChanged;

    public void SetSession(Session? session)
    {
        CurrentSession = session;
        Notify();
    }

    public void SetError(string? error)
    {
        LastError = error;
        Notify();
    }

    public void ClearError()
    {
        LastError = null;
        Notify();
    }

    private void Notify() => StateChanged?.Invoke();
}
