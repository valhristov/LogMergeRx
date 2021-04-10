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

        private readonly FileMap _fileMap = new();

        private readonly ConcurrentDictionary<FileId, long> _offsets = new();

        public IObservable<FileId> ChangedFiles { get; }

        public IObservable<List<LogEntry>> ReadEntries { get; }

        public bool TryGetRelativePath(FileId fileId, out RelativePath relativePath) =>
            _fileMap.TryGetRelativePath(fileId, out relativePath);

        public LogMonitor(AbsolutePath root, string filter = "*.csv")
        {
            _watcher = new ObservableFileSystemWatcher(root, filter);

            ChangedFiles = _watcher.Changed
                .Select(_fileMap.GetOrAddFileId);

            _watcher.Renamed
                .Subscribe(x => _fileMap.TryRename(x.Old, x.New));

            ReadEntries = ChangedFiles
                .Select(ReadToEnd);
        }

        public void Start()
        {
            _watcher.Start(notifyForExistingFiles: true);
        }

        private List<LogEntry> ReadToEnd(FileId fileId)
        {
            List<LogEntry> entries = null;

            if (!_fileMap.TryGetRelativePath(fileId, out var relativePath))
            {
                return null;
            }

            using var stream = File.Open(Path.Combine(_watcher.Root, relativePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            _offsets.AddOrUpdate(fileId,
                fileId => ReadAndGetNewOffset(stream, 0, fileId, out entries),
                (fileId, offset) => ReadAndGetNewOffset(stream, offset, fileId, out entries));

            return entries;

            static long ReadAndGetNewOffset(Stream stream, long offset, FileId fileId, out List<LogEntry> entries)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                entries = CsvParser.Parse(stream, fileId);
                return stream.Position == 0 ? 0 : stream.Position - Environment.NewLine.Length;
            }
        }
    }
}
