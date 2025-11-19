namespace FastLogAnalyzer.Core;

public interface ILogParser
{
    Task<LogEntry[]> ParseAsync(string filePath, CancellationToken cancellationToken);
}