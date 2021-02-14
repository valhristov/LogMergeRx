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
        private readonly ObservableFileSystemWatcher _watcher;

        private readonly Cache<string, CsvReader> _readers =
            new Cache<string, CsvReader>(_ => new CsvReader());

        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MainWindowViewModel();

            DataContext = viewModel;

            var dialog = new OpenFileDialog { Multiselect = false, };

            if (dialog.ShowDialog() == true &&
                dialog.FileNames.Length > 0)
            {
                _watcher = new ObservableFileSystemWatcher(
                    Path.GetDirectoryName(dialog.FileName),
                    Path.GetFileName(dialog.FileName));

                Observable.Merge(_watcher.Changed, _watcher.Created)
                    .SelectMany(ReadToEnd)
                    .ObserveOnUIDispatcher()
                    .Subscribe(viewModel.ItemsSource.Add);

                _watcher.Start(notifyForExistingFiles: true);
            }
        }

        private IEnumerable<LogEntry> ReadToEnd(FileSystemEventArgs args)
        {
            using var stream = File.Open(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return _readers.Get(args.FullPath).Read(stream);
        }
    }
}
