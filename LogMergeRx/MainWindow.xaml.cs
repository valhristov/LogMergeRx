using LogMergeRx.LogViewer;
using LogMergeRx.Model;
using LogMergeRx.Rx;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private readonly Cache<string, ObservableFileSystemWatcher> _watchers;

        private readonly ILogger _logger = new DebugLogger();

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

            _watchers = new Cache<string, ObservableFileSystemWatcher>(
                path => new ObservableFileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path)));

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
                var viewModel = new LogViewerViewModel();

                var stopwatch = new Stopwatch();

                var watcher = _watchers.Get(dialog.FileName);

                _logger.Log(LogLevel.Information, "Create watcher: {0}", stopwatch.ElapsedMilliseconds);

                Observable.Merge(watcher.Changed, watcher.Created)
                    .SelectMany(ReadToEnd)
                    .ObserveOnUIDispatcher()
                    .Subscribe(viewModel.ItemsSource.Add);

                watcher.Start(notifyForExistingFiles: true);

                _logger.Log(LogLevel.Information, "Watcher started: {0}", stopwatch.ElapsedMilliseconds);

                ViewModel.LogViewers.Add(viewModel);
            }
        }
    }

    public class DebugLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new LoggerScope();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Debug.WriteLine(formatter(state, exception));
        }

        private class LoggerScope : IDisposable
        {
            public void Dispose() { }
        }
    }
}
