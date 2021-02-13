using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using LogMergeRx.Model;
using LogMergeRx.Rx;
using Microsoft.Win32;
using Reactive.Bindings.Extensions;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private readonly ObservableFileSystemWatcher _watcher;
        private readonly FileMonitor _monitor = new FileMonitor();

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
                    .SelectMany(args => _monitor.Read(args.FullPath))
                    .ObserveOnUIDispatcher()
                    .Subscribe(viewModel.ItemsSource.Add);

                _watcher.Start();
                _watcher.NotifyForExistingFiles();
            }
        }
    }
}
