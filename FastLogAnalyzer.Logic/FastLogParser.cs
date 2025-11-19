using System.Globalization;
using FastLogAnalyzer.Core;

namespace FastLogAnalyzer.Logic;

public class FastLogParser : ILogParser
{
    // Async method handles ONLY data retrieval
    public async Task<LogEntry[]> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        // 1. Read file asynchronously (IO)
        string fileContent = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // 2. Pass data to synchronous method for processing (CPU)
        // No state machine here, so Span works without errors.
        return ParseMemory(fileContent.AsMemory());
    }

    // Synchronous method for working with SPAN
    private LogEntry[] ParseMemory(ReadOnlyMemory<char> memory)
    {
        var result = new List<LogEntry>(5_000_000);
        int start = 0;

        while (start < memory.Length)
        {
            // Span is safe here as method is not async
            var slice = memory.Slice(start);
            var span = slice.Span;

            int lineLength = span.IndexOf('\n');
            if (lineLength == -1)
            {
                lineLength = span.Length;
            }

            if (lineLength > 0)
            {
                // Pass Slice (Memory) to keep reference without copying
                ParseLine(slice.Slice(0, lineLength), result);
            }

            start += lineLength + 1;
        }

        return result.ToArray();
    }

    private void ParseLine(ReadOnlyMemory<char> lineMemory, List<LogEntry> result)
    {
        var span = lineMemory.Span;

        // --- Zero Allocation Parsing ---
        
        // 1. Date (first column)
        int firstComma = span.IndexOf(',');
        var dateSpan = span.Slice(0, firstComma);
        // Parse date directly from Span! (.NET Core feature)
        DateTime date = DateTime.Parse(dateSpan, CultureInfo.InvariantCulture);

        // 2. Level
        int secondComma = span.Slice(firstComma + 1).IndexOf(',') + firstComma + 1;
        // Save Level as Memory slice (no string allocation)
        var levelMemory = lineMemory.Slice(firstComma + 1, secondComma - firstComma - 1);

        // 3. Message
        int thirdComma = span.Slice(secondComma + 1).IndexOf(',') + secondComma + 1;
        var messageMemory = lineMemory.Slice(secondComma + 1, thirdComma - secondComma - 1);

        // 4. IP (Skip)
        int fourthComma = span.Slice(thirdComma + 1).IndexOf(',') + thirdComma + 1;

        // 5. Response Time
        // Trim('\r') needed as ReadAllText keeps \r before \n on Windows
        var responseTimeSpan = span.Slice(fourthComma + 1).Trim('\r');
        int responseTime = int.Parse(responseTimeSpan);

        result.Add(new LogEntry(date, levelMemory, messageMemory, responseTime));
    }
}