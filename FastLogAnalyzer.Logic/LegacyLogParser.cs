using FastLogAnalyzer.Core;

namespace FastLogAnalyzer.Logic;

public class LegacyLogParser : ILogParser
{
    public async Task<LogEntry[]> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        var result = new LogEntry[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(',');
            var date = DateTime.Parse(parts[0]);
            var level = parts[1];
            var message = parts[2];
            var responseTime = int.Parse(parts[4]);

            result[i] = new LogEntry(
                date,
                level.AsMemory(),
                message.AsMemory(),
                responseTime
            );
        }
        return result;
    }
}