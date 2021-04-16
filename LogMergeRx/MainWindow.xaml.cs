using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using LogMergeRx.Model;

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

            var pathResult = GetMonitorDirectory();
            if (pathResult.IsFailure)
            {
                Close();
                return;
            }

            ViewModel = new MainWindowViewModel();

            _monitor = new LogMonitor(pathResult.ValueOrThrow());

            Title = $"LogMerge {pathResult.ValueOrThrow()}";

            _monitor.ChangedFiles
                .ObserveOnDispatcher()
                .Subscribe(fileId =>
                    _monitor
                        .GetRelativePath(fileId)
                        .Match(
                            // add changed files to the filter
                            relativePath => ViewModel.AddFileToFilter(fileId, relativePath),
                            errors => Logger.Log(errors.FirstOrDefault(), "Error occurred: {0}")));

            _monitor.RenamedFiles
                .ObserveOnDispatcher()
                .Subscribe(fileId =>
                    _monitor
                        .GetRelativePath(fileId)
                        .Match(
                            // update renamed file names. File ID remains the same
                            relativePath => ViewModel.ChangeFileName(fileId, relativePath),
                            errors => Logger.Log(errors.FirstOrDefault(), "Error occurred: {0}")));

            _monitor.ReadEntries
                .ObserveOnDispatcher()
                .Subscribe(ViewModel.ItemsSource.AddRange); // read all content of created or changed files

            _monitor.Start();

            ViewModel.SelectedFiles
                .ToObservable()
                .Subscribe(args => AllFiles.SelectedItems.Sync(args)); // synchronize VM selection with listbox
        }

        private static Result<AbsolutePath> GetMonitorDirectory()
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
