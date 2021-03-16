using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using LogMergeRx.Model;
using LogMergeRx.Rx;

namespace LogMergeRx
{
    public class LogMonitor
    {
        private readonly ObservableFileSystemWatcher _watcher;

        private readonly ConcurrentDictionary<FilePath, long> _offsets =
            new ConcurrentDictionary<FilePath, long>();

        public IObservable<FilePath> ChangedFiles { get; }

        public IObservable<List<LogEntry>> ReadEntries { get; }

        public LogMonitor(string path, string filter = "*.csv")
        {
            _watcher = new ObservableFileSystemWatcher(path, filter);

            ChangedFiles = _watcher.Changed;

            ReadEntries = ChangedFiles
                .Select(ReadToEnd);
        }

        public void Start()
        {
            _watcher.Start(notifyForExistingFiles: true);
        }

        private List<LogEntry> ReadToEnd(FilePath path)
        {
            List<LogEntry> entries = null;

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            _offsets.AddOrUpdate(path,
                fullName => ReadAndGetNewOffset(stream, 0, fullName, out entries),
                (fullName, offset) => ReadAndGetNewOffset(stream, offset, fullName, out entries));

            return entries;

            static long ReadAndGetNewOffset(Stream stream, long offset, string fullName, out List<LogEntry> entries)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                entries = CsvParser.Parse(stream, fullName);
                return stream.Position == 0 ? 0 : stream.Position - Environment.NewLine.Length;
            }
        }
    }
}
