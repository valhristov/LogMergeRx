using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using LogMergeRx.Model;

namespace LogMergeRx;

public static class CsvParser3
{
    public static ImmutableArray<LogEntry> Parse(Stream stream, FileId fileId) =>
        ReadToEnd(stream, fileId)
            .ToImmutableArray();

    private static IEnumerable<LogEntry> ReadToEnd(Stream stream, FileId fileId)
    {
        using var textReader = new StreamReader(stream, leaveOpen: true);

        // Skip header if at start of stream
        if (stream.Position == 0)
        {
            textReader.ReadLine();
        }

        int? threadOffset = null;
        var accumulatedLines = new List<string>();

        while (true)
        {
            var line = textReader.ReadLine();

            if (line == null)
            {
                // End of stream - emit last entry if any
                if (accumulatedLines.Count > 0)
                {
                    var entry = TryParseAccumulatedEntry(accumulatedLines, fileId, ref threadOffset);
                    if (entry != null)
                        yield return entry;
                }
                yield break;
            }

            if (IsNewEntryStart(line))
            {
                // Emit previous entry if any
                if (accumulatedLines.Count > 0)
                {
                    var entry = TryParseAccumulatedEntry(accumulatedLines, fileId, ref threadOffset);
                    if (entry != null)
                        yield return entry;
                    accumulatedLines.Clear();
                }

                // Start new entry
                accumulatedLines.Add(line);
            }
            else if (accumulatedLines.Count > 0)
            {
                // Continuation of current entry
                accumulatedLines.Add(line);
            }
            // else: ignore lines before first valid entry
        }
    }

    private static bool IsNewEntryStart(string line)
    {
        // Check if line starts with valid date: "2026-03-15 00:03:52,076"
        if (line.Length < 25 || line[0] != '"')
            return false;

        if (!char.IsDigit(line[1]) || !char.IsDigit(line[2]) ||
            !char.IsDigit(line[3]) || !char.IsDigit(line[4]))
            return false;

        return line[5] == '-' && line[8] == '-' && line[11] == ' ' &&
               line[14] == ':' && line[17] == ':' && line[20] == ',';
    }

    private static LogEntry TryParseAccumulatedEntry(List<string> lines, FileId fileId, ref int? threadOffset)
    {
        try
        {
            // Parse fields from the first line only
            var firstLine = lines[0];
            var fields = ParseFields(firstLine);

            if (fields.Count < 6)
                return null;

            // Detect threadOffset on first row (check if field 2 is a valid level)
            if (threadOffset == null)
            {
                var testLevel = ParseLevel(fields[2].Trim());
                threadOffset = testLevel == LogLevel.UNKNOWN ? 1 : 0;
            }

            var offset = threadOffset.Value;
            var date = fields[0];
            var level = ParseLevel(fields[2 + offset].Trim());
            var source = fields[3 + offset].Trim();
            
            // Message is field at index (4 + offset)
            var messageFieldIndex = 4 + offset;
            if (messageFieldIndex >= fields.Count)
                return null;

            var message = fields[messageFieldIndex];

            // If there are more lines, append them to the message (they're continuations)
            if (lines.Count > 1)
            {
                var messageBuilder = new StringBuilder(message);
                for (int i = 1; i < lines.Count; i++)
                {
                    messageBuilder.Append("\r\n");
                    messageBuilder.Append(lines[i]);
                }
                message = messageBuilder.ToString();
            }

            // Check if there's an ExceptionString field (last field after message)
            var exceptionFieldIndex = messageFieldIndex + 1;
            var exceptionString = exceptionFieldIndex < fields.Count
                ? fields[exceptionFieldIndex].Trim()
                : "";

            return LogEntry.Create(fileId, date, level, source, message, exceptionString);
        }
        catch
        {
            return null;
        }
    }

    private static List<string> ParseFields(string line)
    {
        var fields = new List<string>();
        var pos = 0;

        while (pos < line.Length)
        {
            // Skip to opening quote
            var quoteStart = line.IndexOf('"', pos);
            if (quoteStart == -1)
                break;

            // Find closing quote
            var quoteEnd = line.IndexOf('"', quoteStart + 1);
            if (quoteEnd == -1)
                break;

            // Extract field content (without quotes)
            var field = line.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            fields.Add(field);

            pos = quoteEnd + 1;
        }

        return fields;
    }

    private static LogLevel ParseLevel(string level) =>
        level.ToUpperInvariant() switch
        {
            "ERROR" or "ERROR " or "ERR" => LogLevel.ERROR,
            "WARN" or "WARN  " or "WRN" => LogLevel.WARN,
            "INFO" or "INFO  " or "INF" => LogLevel.INFO,
            "NOTICE" or "NOT" => LogLevel.NOTICE,
            "DEBUG" or "DEBUG " or "DBG" => LogLevel.DEBUG,
            _ => LogLevel.UNKNOWN
        };
}