using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using LogMergeRx.Model;
using Neat.Results;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        // Need to keep a reference to prevent the GC from collecting the monitor
        private readonly LogMonitor _monitor;

        private MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(
                SystemCommands.CloseWindowCommand,
                (s, e) => SystemCommands.CloseWindow(this)));
            CommandBindings.Add(new CommandBinding(
                SystemCommands.RestoreWindowCommand,
                (s, e) => SystemCommands.RestoreWindow(this),
                (s, e) => e.CanExecute = WindowState == WindowState.Maximized));
            CommandBindings.Add(new CommandBinding(
                SystemCommands.MaximizeWindowCommand,
                (s, e) => SystemCommands.MaximizeWindow(this),
                (s, e) => e.CanExecute = WindowState != WindowState.Maximized));
            CommandBindings.Add(new CommandBinding(
                SystemCommands.MinimizeWindowCommand,
                (s, e) => SystemCommands.MinimizeWindow(this)));

            ViewModel = new MainWindowViewModel();

            ViewModel.SelectedFiles
                .ToObservable()
                .Subscribe(args => AllFiles.SelectedItems.Sync(args)); // synchronize VM selection with listbox

            _monitor = GetLogsPath().Value(
                logsPath =>
                {
                    var monitor = new LogMonitor(logsPath);

                    monitor.ChangedFiles
                        .ObserveOnDispatcher()
                        // Add changed files to the filter
                        .Subscribe(ViewModel.AddFileToFilter);

                    monitor.RenamedFiles
                        .ObserveOnDispatcher()
                        // Update renamed file names. File ID remains the same
                        .Subscribe(ViewModel.UpdateFileName);

                    monitor.ReadEntries
                        .ObserveOnDispatcher()
                        // Add new entries
                        .Subscribe(ViewModel.AddItems);

                    monitor.Start();

                    Title = $"LogMerge: {logsPath}";

                    return monitor;
                },
                errors =>
                {
                    Close();
                    return null;
                });
        }

        private static Result<AbsolutePath> GetLogsPath()
        {
            using var dialog = new FolderBrowserDialog();
            return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? Result.Success((AbsolutePath)dialog.SelectedPath)
                : Result.Failure<AbsolutePath>("User did not choose a directory.");
        }

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.SelectedFiles.Sync(e); // synchronize listbox selection with VM
    }
}
