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

        public ObservableFileSystemWatcher(AbsolutePath root, string filter)
        {
            Root = root;

            InitFileSystemWatcher(root, filter.StartsWith("*") ? filter : $"*{filter}");

            var changed = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _fsw.Changed += h, h => _fsw.Changed -= h)
                .Select(x => x.EventArgs.FullPath)
                .Select(Logger.Log<string>("File changed '{0}'"))
                .Select(path => RelativePath.FromPathAndRoot(root, path));

            var created = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _fsw.Created += h, h => _fsw.Created -= h)
                .Select(x => x.EventArgs.FullPath)
                .Select(Logger.Log<string>("File created '{0}'"))
                .Select(path => RelativePath.FromPathAndRoot(root, path));

            Changed = Observable.Merge(_existing, changed, created);
        }

        private void InitFileSystemWatcher(string directoryPath, string filter)
        {
            _fsw = new FileSystemWatcher
            {
                Path = directoryPath,
                Filter = filter,
                NotifyFilter = NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
            };

            // FSW sometimes makes errors which stop the watching... When an
            // error occurs, we create a new watcher.
            _fsw.Error += OnFileSystemWatcherError;
        }

        private void OnFileSystemWatcherError(object sender, ErrorEventArgs e)
        {
            var fsw = _fsw;

            InitFileSystemWatcher(fsw.Path, fsw.Filter);

            fsw.Error -= OnFileSystemWatcherError;
            fsw.EnableRaisingEvents = false;
            fsw.Dispose();
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
