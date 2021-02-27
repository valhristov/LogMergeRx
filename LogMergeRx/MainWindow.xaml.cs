using LogMergeRx.LogViewer;
using LogMergeRx.Model;
using LogMergeRx.Rx;
using Microsoft.Win32;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private readonly Cache<string, ObservableFileSystemWatcher> _watchers =
            new Cache<string, ObservableFileSystemWatcher>(
                path => new ObservableFileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path)));

        private readonly Cache<string, CsvReader> _readers =
            new Cache<string, CsvReader>(_ => new CsvReader());

        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            private set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
        }

        private IEnumerable<LogEntry> ReadToEnd(FileSystemEventArgs args)
        {
            using var stream = File.Open(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return _readers.Get(args.FullPath).Read(stream);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Multiselect = false, };

            if (dialog.ShowDialog() == true &&
                dialog.FileNames.Length > 0)
            {
                var viewModel = new LogViewerViewModel(Path.GetFileName(dialog.FileName));

                var watcher = _watchers.Get(dialog.FileName);

                Observable.Merge(watcher.Changed, watcher.Created)
                    .Select(ReadToEnd)
                    .ObserveOnUIDispatcher()
                    .Subscribe(viewModel.ItemsSource.AddRange);

                watcher.Start(notifyForExistingFiles: true);

                ViewModel.LogViewers.Add(viewModel);
            }
        }
    }
}
