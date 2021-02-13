using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LogMergeRx.Rx
{
    public class ObservableFileSystemWatcher
    {
        private readonly FileSystemWatcher _fsw;

        public readonly Subject<FileSystemEventArgs> _existing =
            new Subject<FileSystemEventArgs>();

        public IObservable<FileSystemEventArgs> Changed { get; }

        public IObservable<FileSystemEventArgs> Created { get; }

        public ObservableFileSystemWatcher(string directoryPath, string filter)
        {
            _fsw = new FileSystemWatcher
            {
                Path = directoryPath,
                Filter = filter,
                NotifyFilter = NotifyFilters.LastWrite,
            };

            Changed = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _fsw.Changed += h, h => _fsw.Changed -= h)
                .Select(x => x.EventArgs);

            Created = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _fsw.Created += h, h => _fsw.Created -= h)
                    .Select(x => x.EventArgs)
                    .Merge(_existing);
        }

        public void Start() =>
            _fsw.EnableRaisingEvents = true;

        public void Stop() =>
            _fsw.EnableRaisingEvents = false;

        public void NotifyForExistingFiles()
        {
            foreach (var args in Directory.GetFiles(_fsw.Path, _fsw.Filter).Select(ArgsFromPath))
            {
                _existing.OnNext(args);
            }
        }

        private FileSystemEventArgs ArgsFromPath(string fullPath) =>
            new FileSystemEventArgs(
                WatcherChangeTypes.Created,
                Path.GetDirectoryName(fullPath),
                Path.GetFileName(fullPath));
    }
}
