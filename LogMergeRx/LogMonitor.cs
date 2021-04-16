using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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

        public IObservable<FileId> RenamedFiles { get; }

        public IObservable<ImmutableList<LogEntry>> ReadEntries { get; }

        public Result<RelativePath> GetRelativePath(FileId fileId) =>
            _fileMap.GetRelativePath(fileId);

        public LogMonitor(AbsolutePath root, string filter = "*.csv")
        {
            _watcher = new ObservableFileSystemWatcher(root, filter);

            ChangedFiles = _watcher.Changed
                .Select(_fileMap.GetOrAddFileId);

            RenamedFiles = _watcher.Renamed
                .Select(x => _fileMap.Rename(x.Old, x.New))
                .Where(result => !result.IsFailure)
                .Select(result => result.ValueOrThrow());

            ReadEntries = Observable.Merge(RenamedFiles, ChangedFiles)
                .Select(ReadToEnd);
        }

        public void Start()
        {
            _watcher.Start(notifyForExistingFiles: true);
        }

        private ImmutableList<LogEntry> ReadToEnd(FileId fileId)
        {
            return _fileMap
                .GetRelativePath(fileId)
                .Select(
                    relativePath =>
                    {
                        try
                        {
                            using var stream = File.Open(relativePath.ToAbsolute(_watcher.Root), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                            ImmutableList<LogEntry> entries = null;
                            _offsets.AddOrUpdate(fileId,
                                fileId => ReadAndGetNewOffset(stream, 0, fileId, out entries),
                                (fileId, offset) => ReadAndGetNewOffset(stream, offset, fileId, out entries));

                            return entries;
                        }
                        catch
                        {
                            return ImmutableList<LogEntry>.Empty; // TODO: log
                        }
                    },
                    errors => ImmutableList<LogEntry>.Empty); // TODO: log


            static long ReadAndGetNewOffset(Stream stream, long offset, FileId fileId, out ImmutableList<LogEntry> entries)
            {
                if (stream.Length <= offset) // The file was renamed, don't read anything, wait for renaming notification to arrive
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
