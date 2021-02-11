using System.Windows;
using Microsoft.Win32;

namespace LogMergeRx
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel CreateViewModel()
        {
            var dialog = new OpenFileDialog { Multiselect = true, };

            if (dialog.ShowDialog() == true &&
                dialog.FileNames.Length > 0)
            {
                var viewModel = new MainWindowViewModel();
                viewModel.LoadFiles(dialog.FileNames);
                return viewModel;
            }

            return null;
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = CreateViewModel();
        }
    }
}
