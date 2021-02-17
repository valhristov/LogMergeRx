
using System.Collections.ObjectModel;
using LogMergeRx.LogViewer;

namespace LogMergeRx
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            SelectedViewerIndex = new ObservableProperty<int>(0);
        }

        public ObservableProperty<int> SelectedViewerIndex { get; }

        public ObservableCollection<LogViewerViewModel> LogViewers { get; } =
            new ObservableCollection<LogViewerViewModel>();
    }
}