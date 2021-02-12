using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class MainWindowViewModel
    {
        public ObservableCollection<LogEntry> ItemsSource { get; }
        public ObservableProperty<bool> ShowErrors { get; }
        public ObservableProperty<bool> ShowWarnings { get; }
        public ObservableProperty<bool> ShowNotices { get; }
        public ObservableProperty<bool> ShowInfos { get; }
        public ObservableProperty<string> IncludeRegex { get; }
        public ObservableProperty<string> ExcludeRegex { get; }
        public ObservableProperty<string> SearchRegex { get; }
        public ObservableProperty<int> ScrollToIndex { get; }
        public ActionCommand NextIndex { get; }
        public ActionCommand PrevIndex { get; }

        private ICollectionView ItemsSourceView =>
            CollectionViewSource.GetDefaultView(ItemsSource);

        private IEnumerable<(int Index, LogEntry Item)> ItemsAndIndexes =>
            ItemsSourceView.Cast<LogEntry>().Select((item, index) => (index, item));

        public MainWindowViewModel(IEnumerable<LogEntry> items)
        {
            ItemsSource = new ObservableCollection<LogEntry>(items);

            ShowErrors = new ObservableProperty<bool>(true);
            ShowWarnings = new ObservableProperty<bool>(true);
            ShowNotices = new ObservableProperty<bool>(true);
            ShowInfos = new ObservableProperty<bool>(true);
            IncludeRegex = new ObservableProperty<string>(string.Empty);
            ExcludeRegex = new ObservableProperty<string>(string.Empty);
            SearchRegex = new ObservableProperty<string>(string.Empty);
            ScrollToIndex = new ObservableProperty<int>(0);

            NextIndex = new ActionCommand(_ => ScrollIntoView(ScrollToIndex.Value, ListSortDirection.Ascending));
            PrevIndex = new ActionCommand(_ => ScrollIntoView(ScrollToIndex.Value, ListSortDirection.Descending));

            ItemsSourceView.Filter = Filter;

            Observable.Merge(ShowErrors, ShowWarnings, ShowNotices, ShowInfos).Subscribe(_ => ItemsSourceView.Refresh());
            Observable.Merge(IncludeRegex, ExcludeRegex).Subscribe(_ => ItemsSourceView.Refresh());

            SearchRegex.Subscribe(_ => ScrollIntoView(-1, ListSortDirection.Ascending));
        }

        private void ScrollIntoView(int startIndex, ListSortDirection direction)
        {
            var regex = RegexCache.GetRegex(SearchRegex.Value);

            var result = direction == ListSortDirection.Ascending
                ? ItemsAndIndexes.Skip(startIndex + 1).FirstOrDefault(x => regex.IsMatch(x.Item.Message))
                : ItemsAndIndexes.Take(startIndex).LastOrDefault(x => regex.IsMatch(x.Item.Message));

            if (result.Item != null) // match found
            {
                ScrollToIndex.Value = result.Index;
            }
        }

        private bool Filter(object o)
        {
            return o is LogEntry log && FilterByLevel(log) && FilterByInclude(log) && FilterByExclude(log);

            bool FilterByLevel(LogEntry log) =>
                ShowErrors.Value && string.Equals(log.Level, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                ShowWarnings.Value && string.Equals(log.Level, "WARN", StringComparison.OrdinalIgnoreCase) ||
                ShowInfos.Value && string.Equals(log.Level, "INFO", StringComparison.OrdinalIgnoreCase) ||
                ShowNotices.Value && string.Equals(log.Level, "NOTICE", StringComparison.OrdinalIgnoreCase);

            bool FilterByInclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(IncludeRegex.Value) || RegexCache.GetRegex(IncludeRegex.Value).IsMatch(log.Message);

            bool FilterByExclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(ExcludeRegex.Value) || !RegexCache.GetRegex(ExcludeRegex.Value).IsMatch(log.Message);
        }
    }
}
