using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LogMergeRx.Model;
using Microsoft.VisualBasic.FileIO;
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
                var viewModel = new MainWindowViewModel(ParseFiles(dialog.FileNames).OrderBy(x => x.Date));
                return viewModel;
            }

            return null;
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = CreateViewModel();
        }

        private static IEnumerable<LogEntry> ParseFiles(string[] paths)
        {
            foreach (var file in paths)
            {
                var fileName = System.IO.Path.GetFileName(file);
                using (var parser = new TextFieldParser(file) { TextFieldType = FieldType.Delimited })
                {
                    parser.SetDelimiters(";");
                    _ = parser.ReadFields(); // read the headers
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        yield return new LogEntry(
                            fileName,
                            DateTime.ParseExact(fields[0], "yyyy-MM-dd HH:mm:ss,fff", null),
                            fields[2].Trim(),
                            fields[3].Trim(),
                            fields[4]);
                    }
                }
            }
        }

    }
}
