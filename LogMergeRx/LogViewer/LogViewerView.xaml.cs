using System.Linq;
using System.Windows.Controls;

namespace LogMergeRx.LogViewer
{
    public partial class LogViewerView : UserControl
    {
        public LogViewerView()
        {
            InitializeComponent();
        }

        private LogViewerViewModel ViewModel =>
            (LogViewerViewModel)DataContext;

        private void AllFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedFiles.RemoveAll(file => e.RemovedItems.Contains(file));
            ViewModel.SelectedFiles.AddRange(e.AddedItems.OfType<string>());
        }
    }
}
