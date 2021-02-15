
using System.Collections.ObjectModel;
using LogMergeRx.LogViewer;

namespace LogMergeRx
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {

        }

        public ObservableCollection<LogViewerViewModel> LogViewers { get; } =
            new ObservableCollection<LogViewerViewModel>();
    }
}