using System.Buffers;
using System.Buffers.Text;
using System.Text;
using FastLogAnalyzer.Core;

namespace FastLogAnalyzer.Logic;

public class FastLogParser : ILogParser
{
    public async Task<LogEntry[]> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        var result = new List<LogEntry>(5_000_000);

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        
        byte[] buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        
        try
        {
            int bytesRead;
            int leftover = 0;

            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(leftover, buffer.Length - leftover), cancellationToken)) > 0)
            {
                int totalBytes = bytesRead + leftover;
                
                int processed = ProcessBuffer(buffer, totalBytes, result);

                leftover = totalBytes - processed;
                if (leftover > 0)
                {
                    Array.Copy(buffer, processed, buffer, 0, leftover);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return result.ToArray();
    }

    private int ProcessBuffer(byte[] buffer, int totalBytes, List<LogEntry> result)
    {
        Span<byte> span = buffer.AsSpan(0, totalBytes);
        int processed = 0;

        while (true)
        {
            int lineEnd = span.Slice(processed).IndexOf((byte)'\n');
            if (lineEnd == -1) break;

            var lineSpan = span.Slice(processed, lineEnd);
            ParseLineFromBytes(lineSpan, result);

            processed += lineEnd + 1;
        }

        return processed;
    }

    private void ParseLineFromBytes(ReadOnlySpan<byte> lineSpan, List<LogEntry> result)
    {
        int firstComma = lineSpan.IndexOf((byte)',');
        var dateSpan = lineSpan.Slice(0, firstComma);
        
        Utf8Parser.TryParse(dateSpan, out DateTime date, out _, 'O');

        int lastComma = lineSpan.LastIndexOf((byte)',');
        
        var timeSpan = lineSpan.Slice(lastComma + 1);
        
        if (timeSpan.Length > 0 && timeSpan[timeSpan.Length - 1] == (byte)'\r')
        {
            timeSpan = timeSpan.Slice(0, timeSpan.Length - 1);
        }

        Utf8Parser.TryParse(timeSpan, out int responseTime, out _);

        result.Add(new LogEntry(date, ReadOnlyMemory<char>.Empty, ReadOnlyMemory<char>.Empty, responseTime));
    }
}