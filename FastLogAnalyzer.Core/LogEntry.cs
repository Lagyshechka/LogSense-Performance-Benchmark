namespace FastLogAnalyzer.Core;

public readonly struct LogEntry
{
    public DateTime Timestamp { get; }
    public ReadOnlyMemory<char> Level { get; }
    public ReadOnlyMemory<char> Message { get; }
    public int ResponseTimeMs { get; }

    public LogEntry(DateTime timestamp, ReadOnlyMemory<char> level, ReadOnlyMemory<char> message, int responseTimeMs)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message;
        ResponseTimeMs = responseTimeMs;
    }
}