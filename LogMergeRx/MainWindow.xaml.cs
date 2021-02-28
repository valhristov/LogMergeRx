using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using LogMergeRx.LogViewer;
using LogMergeRx.Model;
using LogMergeRx.Rx;
using Microsoft.Win32;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private readonly Cache<string, ObservableFileSystemWatcher> _watchers =
            new Cache<string, ObservableFileSystemWatcher>(
                path => new ObservableFileSystemWatcher(Path.GetDirectoryName(path), Path.GetExtension(path)));

        private readonly Cache<string, CsvReader> _readers =
            new Cache<string, CsvReader>(fullPath => new CsvReader(Path.GetFileName(fullPath)));

        private DateTime _appStart;

        public MainWindow()
        {
            _appStart = DateTime.UtcNow;

            InitializeComponent();

            var dialog = new OpenFileDialog { Multiselect = false, };

            if (dialog.ShowDialog() == true &&
                dialog.FileNames.Length > 0)
            {
                var viewModel = new LogViewerViewModel();
                DataContext = viewModel;

                var watcher = _watchers.Get(dialog.FileName);

                Observable.Merge(watcher.Changed, watcher.Created)
                    .Where(FromAppStart)
                    .Select(args => args.Name)
                    .ObserveOnDispatcher()
                    .Subscribe(viewModel.AddFileToFilter);

                Observable.Merge(watcher.Changed, watcher.Created)
                    .Where(FromAppStart)
                    .Select(ReadToEnd)
                    .ObserveOnDispatcher()
                    .Subscribe(viewModel.ItemsSource.AddRange);

                watcher.Start(notifyForExistingFiles: true);
            }
        }

        private bool FromAppStart(FileSystemEventArgs args) =>
            true;
            //File.GetLastWriteTimeUtc(args.FullPath) >= _appStart;

        private IEnumerable<LogEntry> ReadToEnd(FileSystemEventArgs args)
        {
            using var x = Meter.MeasureBegin(args.Name);

            using var stream = File.Open(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return _readers.Get(args.FullPath).Read(stream);
        }
    }
}
