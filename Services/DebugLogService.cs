namespace OpenBoxesMobile.Blazor.Services;

public sealed class DebugLogService
{
    private readonly List<DebugLogEntry> _entries = [];
    private readonly object _gate = new();

    public event Action? Changed;

    public void Info(string source, string message) => Add("INFO", source, message);
    public void Error(string source, string message) => Add("ERROR", source, message);

    public IReadOnlyList<DebugLogEntry> GetEntries()
    {
        lock (_gate)
        {
            return _entries.ToList();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _entries.Clear();
        }

        Changed?.Invoke();
    }

    private void Add(string level, string source, string message)
    {
        lock (_gate)
        {
            _entries.Add(new DebugLogEntry(DateTimeOffset.Now, level, source, message));

            if (_entries.Count > 200)
            {
                _entries.RemoveRange(0, _entries.Count - 200);
            }
        }

        Changed?.Invoke();
    }
}

public sealed record DebugLogEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Source,
    string Message);
