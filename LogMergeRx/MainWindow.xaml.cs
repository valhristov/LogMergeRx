using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using LogMergeRx.Model;
using Microsoft.Win32;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private readonly LogMonitor _monitor;

        private MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            if (!TryGetDirectoryToRead(out var path))
            {
                Close();
                return;
            }

            Title = $"Monitoring: {path}";

            ViewModel = new MainWindowViewModel();

            _monitor = new LogMonitor(AbsolutePath.FromFullPath(path));

            _monitor.ChangedFiles
                .ObserveOnDispatcher()
                .Subscribe(ViewModel.AddFileToFilter); // add changed files to the filter

            _monitor.ReadEntries
                .ObserveOnDispatcher()
                .Subscribe(ViewModel.ItemsSource.AddRange); // read all content of created or changed files

            _monitor.Start();

            ViewModel.SelectedFiles
                .ToObservable()
                .Subscribe(args => AllFiles.SelectedItems.Sync(args)); // synchronize VM selection with listbox
        }

        private bool TryGetDirectoryToRead(out string path)
        {
            using var dialog = new FolderBrowserDialog();

            path = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? dialog.SelectedPath
                : null;

            return path != null;
        }

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.SelectedFiles.Sync(e); // synchronize listbox selection with VM
    }
}
