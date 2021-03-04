using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using LogMergeRx.Model;
using LogMergeRx.Rx;
using Microsoft.Win32;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private readonly Cache<string, ObservableFileSystemWatcher> _watchers =
            new Cache<string, ObservableFileSystemWatcher>(
                path => new ObservableFileSystemWatcher(Path.GetDirectoryName(path), "*.csv"));

        private readonly Cache<string, CsvReader> _readers =
            new Cache<string, CsvReader>(fullPath => new CsvReader(Path.GetFileName(fullPath)));

        private MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            var dialog = new OpenFileDialog { Multiselect = false, };
            ViewModel = new MainWindowViewModel();

            if (dialog.ShowDialog() == true &&
                dialog.FileNames.Length > 0)
            {
                var watcher = _watchers.Get(dialog.FileName);

                Observable.Merge(watcher.Changed, watcher.Created)
                    .Select(args => args.Name)
                    .ObserveOnDispatcher()
                    .Subscribe(ViewModel.AddFileToFilter);

                Observable.Merge(watcher.Changed, watcher.Created)
                    .Select(ReadToEnd)
                    .ObserveOnDispatcher()
                    .Subscribe(ViewModel.ItemsSource.AddRange);

                ViewModel.SelectedFiles
                    .ToObservable()
                    .Subscribe(args => AllFiles.SelectedItems.Sync(args));

                watcher.Start(notifyForExistingFiles: true);
            }
        }

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.SelectedFiles.Sync(e);

        private IEnumerable<LogEntry> ReadToEnd(FileSystemEventArgs args)
        {
            using var x = Meter.MeasureBegin(args.Name);

            using var stream = File.Open(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return _readers.Get(args.FullPath).Read(stream);
        }
    }
}
