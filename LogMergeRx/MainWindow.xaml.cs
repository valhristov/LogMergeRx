using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

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
