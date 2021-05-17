using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            var command = new ActionCommand(_ => SearchTextBox.Focus());
            InputBindings.Add(new KeyBinding(command, Key.F, ModifierKeys.Control));

            ViewModel = new MainWindowViewModel(DispatcherScheduler.Current);

            ViewModel.SelectedFiles
                .ToObservable()
                .Subscribe(args => AllFiles.SelectedItems.Sync(args)); // synchronize VM selection with listbox

            _monitor = new LogMonitor(App.LogsPath);

            _monitor.ChangedFiles
                .ObserveOnDispatcher()
                // Add changed files to the filter
                .Subscribe(ViewModel.AddFileToFilter);

            _monitor.RenamedFiles
                .ObserveOnDispatcher()
                // Update renamed file names. File ID remains the same
                .Subscribe(ViewModel.UpdateFileName);

            _monitor.ReadEntries
                .ObserveOnDispatcher()
                // Add new entries
                .Subscribe(ViewModel.AddItems);

            _monitor.Start();


            Title = $"LogMerge: {App.LogsPath}";
        }

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.SelectedFiles.Sync(e); // synchronize listbox selection with VM
    }
}
