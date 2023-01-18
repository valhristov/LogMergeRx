using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using LogMergeRx.Model;
using LogMergeRx.Rx;
using Neat.Results;

namespace LogMergeRx
{
    public class LogMonitor
    {
        private readonly ObservableFileSystemWatcher _watcher;

        private readonly FileMap _fileMap = new();

        private readonly ConcurrentDictionary<FileId, long> _offsets = new();

        public IObservable<LogFile> ChangedFiles { get; }

        public IObservable<LogFile> RenamedFiles { get; }

        public IObservable<ImmutableArray<LogEntry>> ReadEntries { get; }

        public LogMonitor(AbsolutePath root, string filter = "*.csv")
        {
            _watcher = new ObservableFileSystemWatcher(root, filter);

            ChangedFiles = _watcher.Changed
                .Select(GetOrAdd);

            RenamedFiles = _watcher.Renamed
                .Select(x => Rename(x.Old, x.New))
                .SelectMany(
                    result => result.Value(Observable.Return, errors => Observable.Empty<LogFile>())); // TODO log

            ReadEntries = Observable.Merge(RenamedFiles, ChangedFiles)
                .Select(ReadToEnd);
        }

        public void Start()
        {
            _watcher.Start(notifyForExistingFiles: true);
        }

        private LogFile GetOrAdd(RelativePath relativePath) =>
            new LogFile(_fileMap.GetOrAddFileId(relativePath), relativePath);

        private Result<LogFile> Rename(RelativePath oldPath, RelativePath newPath) =>
            _fileMap
                .Rename(oldPath, newPath)
                .Select(fileId => Result.Success(new LogFile(fileId, newPath)));

        private ImmutableArray<LogEntry> ReadToEnd(LogFile logFile)
        {
            try
            {
                using var stream = File.Open(logFile.Path.ToAbsolute(_watcher.Root), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var entries = ImmutableArray<LogEntry>.Empty;

                _offsets.AddOrUpdate(logFile.Id,
                    fileId => ReadAndGetNewOffset(stream, 0, fileId, out entries),
                    (fileId, offset) => ReadAndGetNewOffset(stream, offset, fileId, out entries));

                return entries;
            }
            catch (Exception)
            {
                return ImmutableArray<LogEntry>.Empty; // TODO log
            }

            static long ReadAndGetNewOffset(Stream stream, long offset, FileId fileId, out ImmutableArray<LogEntry> entries)
            {
                // The file was probably renamed, don't read anything
                if (stream.Length <= offset)
                {
                    entries = ImmutableArray<LogEntry>.Empty; // TODO log
                    return offset;
                }
                stream.Seek(offset, SeekOrigin.Begin);
                entries = CsvParser.Parse(stream, fileId);
                return stream.Position == 0 ? 0 : stream.Position - Environment.NewLine.Length;
            }
        }
    }
}
