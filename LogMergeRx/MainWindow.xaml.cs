using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();

            if (TryGetDirectoryToRead(out var path))
            {
                var monitor = new LogMonitor(path);

                monitor.ChangedFiles
                    .ObserveOnDispatcher()
                    .Subscribe(ViewModel.AddFileToFilter); // add changed files to the filter

                monitor.ReadEntries
                    .ObserveOnDispatcher()
                    .Subscribe(ViewModel.ItemsSource.AddRange); // read all content of created or changed files

                monitor.Start();
            }

            ViewModel.SelectedFiles
                .ToObservable()
                .Subscribe(args => AllFiles.SelectedItems.Sync(args)); // synchronize VM selection with listbox
        }

        private bool TryGetDirectoryToRead(out string path)
        {
            var dialog = new OpenFileDialog { Multiselect = false, };
            path = dialog.ShowDialog() == true && dialog.FileNames.Length > 0
                ? Path.GetDirectoryName(dialog.FileName)
                : null;
            return path != null;
        }

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.SelectedFiles.Sync(e); // synchronize listbox selection with VM
    }
}
