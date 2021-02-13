using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class FileMonitor
    {
        private readonly ConcurrentDictionary<string, long> _offsets =
            new ConcurrentDictionary<string, long>();

        public IEnumerable<LogEntry> Read(string fullPath)
        {
            using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var offset = _offsets.GetOrAdd(fullPath, 0);

                stream.Seek(offset, SeekOrigin.Begin);

                var result = CsvParser.ReadAll(stream).ToList();

                _offsets.AddOrUpdate(fullPath, 0,
                    (path, original) => stream.Position - Environment.NewLine.Length);

                return result;
            }
        }
    }
}
