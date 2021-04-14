using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LogMergeRx.Model;
using LogMergeRx.Rx;

namespace LogMergeRx
{
    public class LogMonitor
    {
        private readonly ObservableFileSystemWatcher _watcher;

        private readonly FileMap _fileMap = new();

        private readonly ConcurrentDictionary<FileId, long> _offsets = new();
        private readonly Subject<FileId> _renamedFiles = new();

        public IObservable<FileId> ChangedFiles { get; }

        public IObservable<FileId> RenamedFiles { get; }

        public IObservable<ImmutableList<LogEntry>> ReadEntries { get; }

        public bool TryGetRelativePath(FileId fileId, out RelativePath relativePath) =>
            _fileMap.TryGetRelativePath(fileId, out relativePath);

        public LogMonitor(AbsolutePath root, string filter = "*.csv")
        {
            _watcher = new ObservableFileSystemWatcher(root, filter);

            ChangedFiles = _watcher.Changed
                .Select(_fileMap.GetOrAddFileId);

            RenamedFiles = _renamedFiles;

            _watcher.Renamed
                .Subscribe(x =>
                    {
                        if (_fileMap.TryRename(x.Old, x.New))
                        {
                            _renamedFiles.OnNext(_fileMap.GetOrAddFileId(x.New));
                        }
                    });

            ReadEntries = Observable.Merge(RenamedFiles, ChangedFiles)
                .Select(ReadToEnd);
        }

        public void Start()
        {
            _watcher.Start(notifyForExistingFiles: true);
        }

        private ImmutableList<LogEntry> ReadToEnd(FileId fileId)
        {
            // TODO: this should not return invald path
            if (!_fileMap.TryGetRelativePath(fileId, out var relativePath))
            {
                return ImmutableList<LogEntry>.Empty;
            }

            try
            {
                using var stream = File.Open(Path.Combine(_watcher.Root, relativePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                ImmutableList<LogEntry> entries = null;
                _offsets.AddOrUpdate(fileId,
                    fileId => ReadAndGetNewOffset(stream, 0, fileId, out entries),
                    (fileId, offset) => ReadAndGetNewOffset(stream, offset, fileId, out entries));

                return entries;
            }
            catch
            {
                return ImmutableList<LogEntry>.Empty;
            }

            static long ReadAndGetNewOffset(Stream stream, long offset, FileId fileId, out ImmutableList<LogEntry> entries)
            {
                if (stream.Length < offset)
                {
                    entries = ImmutableList<LogEntry>.Empty;
                    return offset;
                }
                stream.Seek(offset, SeekOrigin.Begin);
                entries = CsvParser.Parse(stream, fileId);
                return stream.Position == 0 ? 0 : stream.Position - Environment.NewLine.Length;
            }
        }
    }
}
