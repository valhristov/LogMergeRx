using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Microsoft.VisualBasic.FileIO;

namespace LogMergeRx
{
    public class MainWindowViewModel
    {
        public CollectionViewSource ItemsSource { get; } = new CollectionViewSource();// new FilteredCollection<LogEntry>();

        public ObservableProperty<bool> ShowErrors { get; }
        public ObservableProperty<bool> ShowWarnings { get; }
        public ObservableProperty<bool> ShowNotices { get; }
        public ObservableProperty<bool> ShowInfos { get; }
        public ObservableProperty<string> IncludeRegex { get; }
        public ObservableProperty<string> ExcludeRegex { get; }

        public MainWindowViewModel()
        {
            ShowErrors = new ObservableProperty<bool>(true);
            ShowWarnings = new ObservableProperty<bool>(true);
            ShowNotices = new ObservableProperty<bool>(true);
            ShowInfos = new ObservableProperty<bool>(true);
            IncludeRegex = new ObservableProperty<string>(string.Empty);
            ExcludeRegex = new ObservableProperty<string>(string.Empty);

            Observable.Merge(ShowErrors, ShowWarnings, ShowNotices, ShowInfos).Subscribe(_ => UpdateFilter());
            Observable.Merge(IncludeRegex, ExcludeRegex).Subscribe(_ => UpdateFilter());
        }

        private void UpdateFilter()
        {
            ItemsSource.View.Filter = 
                o => o is LogEntry log && FilterByLevel(log) && FilterByInclude(log) && FilterByExclude(log);

            bool FilterByLevel(LogEntry log) =>
                log.Level == "ERROR" && ShowErrors.Value ||
                log.Level == "WARN" && ShowWarnings.Value ||
                log.Level == "INFO" && ShowInfos.Value ||
                log.Level == "NOTICE" && ShowNotices.Value;

            bool FilterByInclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(IncludeRegex.Value) || Regex.IsMatch(log.Message, IncludeRegex.Value);

            bool FilterByExclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(ExcludeRegex.Value) || !Regex.IsMatch(log.Message, ExcludeRegex.Value);
        }

        public void LoadFiles(string[] paths)
        {
            var entries = ParseFiles(paths).ToList();

            ItemsSource.Source = entries;
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
