using LogMergeRx.Model;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        public static TimeSpan OneSecond { get; } = TimeSpan.FromSeconds(1);
        public static TimeSpan FiveSeconds { get; } = TimeSpan.FromSeconds(5);
        public static TimeSpan OneMinute { get; } = TimeSpan.FromMinutes(1);

        private MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public static RoutedUICommand SetTimeFilterCommand { get; } =
            new RoutedUICommand("Set filter", "SetTimeFilter", typeof(MainWindow));

        public MainWindow(MainWindowViewModel viewModel)
        {
            ViewModel = viewModel;

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

            CommandBindings.Add(new CommandBinding(
                SetTimeFilterCommand,
                (s, e) => ViewModel.DateFilterViewModel.SetStartEnd((e.OriginalSource as FrameworkElement)?.DataContext as LogEntry, e.Parameter),
                (s, e) => e.CanExecute = true));

            var command = new ActionCommand(_ => SearchTextBox.Focus());
            InputBindings.Add(new KeyBinding(command, Key.F, ModifierKeys.Control));

            ViewModel.FileFilterViewModel.SelectedFiles
                .ToObservable()
                .Subscribe(args => AllFiles.SelectedItems.Sync(args)); // synchronize VM selection with listbox

            ViewModel.SourceFilterViewModel.SelectedSources
                .ToObservable()
                .Subscribe(args => AllSources.SelectedItems.Sync(args)); // synchronize VM selection with listbox
        }

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.FileFilterViewModel.SelectedFiles.Sync(e); // synchronize listbox selection with VM

        private void AllSources_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ViewModel.SourceFilterViewModel.SelectedSources.Sync(e); // synchronize listbox selection with VM
    }
}
