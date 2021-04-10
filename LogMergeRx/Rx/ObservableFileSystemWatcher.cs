using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LogMergeRx.Model;

namespace LogMergeRx.Rx
{
    public class ObservableFileSystemWatcher : IDisposable
    {
        private readonly Subject<RelativePath> _existing =
            new Subject<RelativePath>();

        private FileSystemWatcher _fsw;

        public AbsolutePath Root { get; }
        public IObservable<RelativePath> Changed { get; }
        public IObservable<(RelativePath Old, RelativePath New)> Renamed { get; }

        public ObservableFileSystemWatcher(AbsolutePath root, string filter)
        {
            Root = root;

            Logger.Log(root, "Monitoring: '{0}'");

            _fsw = new FileSystemWatcher
            {
                Path = root.Value + "\\",
                Filter = "*.csv", //filter.StartsWith("*") ? filter : $"*{filter}",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                InternalBufferSize = 65532,
            };

            _fsw.Error += (s, e) =>
                Logger.Log(e.GetException().Message, "Detected error: '{0}'");

            var changed = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _fsw.Changed += h, h => _fsw.Changed -= h)
                .Select(x => x.EventArgs.FullPath)
                .Select(path => RelativePath.FromPathAndRoot(root, path))
                .Select(Logger.Log<RelativePath>("File changed '{0}'"))
                ;

            var created = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _fsw.Created += h, h => _fsw.Created -= h)
                .Select(x => x.EventArgs.FullPath)
                .Select(path => RelativePath.FromPathAndRoot(root, path))
                .Select(Logger.Log<RelativePath>("File created '{0}'"))
                ;

            Changed = Observable.Merge(_existing, changed, created);

            Renamed = Observable
                .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(h => _fsw.Renamed += h, h => _fsw.Renamed -= h)
                .Select(x =>
                    (RelativePath.FromPathAndRoot(root, x.EventArgs.OldFullPath),
                     RelativePath.FromPathAndRoot(root, x.EventArgs.FullPath)))
                ;
        }

        public void Start(bool notifyForExistingFiles)
        {
            _fsw.EnableRaisingEvents = true;
            if (notifyForExistingFiles)
            {
                var filePaths = Directory
                    .GetFiles(_fsw.Path, _fsw.Filter, SearchOption.AllDirectories)
                    .Select(fullPath => RelativePath.FromPathAndRoot(Root, fullPath))
                    .Select(Logger.Log<RelativePath>("Existing file '{0}'"))
                    .ToList();
                filePaths.ForEach(_existing.OnNext);
            }
        }

        public void Stop() =>
            _fsw.EnableRaisingEvents = false;

        public void Dispose()
        {
            _fsw.EnableRaisingEvents = false;
            _fsw.Dispose();
        }
    }
}
