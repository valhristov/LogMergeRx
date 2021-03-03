using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class MainWindowViewModel
    {
        private HashSet<string> _selection = new HashSet<string>();

        public WpfObservableRangeCollection<LogEntry> ItemsSource { get; } =
            new WpfObservableRangeCollection<LogEntry>();

        public ObservableProperty<bool> FollowTail { get; }
        public ObservableProperty<bool> ShowErrors { get; }
        public ObservableProperty<bool> ShowWarnings { get; }
        public ObservableProperty<bool> ShowNotices { get; }
        public ObservableProperty<bool> ShowInfos { get; }
        public ObservableProperty<string> IncludeRegex { get; }
        public ObservableProperty<string> ExcludeRegex { get; }
        public ObservableProperty<string> SearchRegex { get; }
        public ObservableProperty<int> ScrollToIndex { get; }
        public ObservableProperty<LogEntry> ScrollToItem { get; }
        public ActionCommand NextIndex { get; }
        public ActionCommand PrevIndex { get; }

        private ListCollectionView ItemsSourceView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(ItemsSource);

        private IEnumerable<(int Index, LogEntry Item)> ItemsAndIndexes =>
            ItemsSourceView.Cast<LogEntry>().Select((item, index) => (index, item));

        public WpfObservableRangeCollection<string> AllFiles { get; } =
            new WpfObservableRangeCollection<string>();

        public WpfObservableRangeCollection<string> SelectedFiles { get; } =
            new WpfObservableRangeCollection<string>();

        public MainWindowViewModel()
        {
            FollowTail = new ObservableProperty<bool>(true);
            ShowErrors = new ObservableProperty<bool>(true);
            ShowWarnings = new ObservableProperty<bool>(true);
            ShowNotices = new ObservableProperty<bool>(true);
            ShowInfos = new ObservableProperty<bool>(true);
            IncludeRegex = new ObservableProperty<string>(string.Empty);
            ExcludeRegex = new ObservableProperty<string>(string.Empty);
            SearchRegex = new ObservableProperty<string>(string.Empty);
            ScrollToIndex = new ObservableProperty<int>(0);
            ScrollToItem = new ObservableProperty<LogEntry>(null);

            NextIndex = new ActionCommand(_ => FindNext(SearchRegex.Value, ScrollToIndex.Value));
            PrevIndex = new ActionCommand(_ => FindPrev(SearchRegex.Value, ScrollToIndex.Value));

            ItemsSourceView.Filter = Filter;
            ItemsSourceView.CustomSort = new LogEntryDateComparer();

            Observable
                .Merge(ShowErrors, ShowWarnings, ShowNotices, ShowInfos)
                .Subscribe(_ => ItemsSourceView.Refresh());
            Observable
                .Merge(IncludeRegex, ExcludeRegex)
                .Subscribe(_ => ItemsSourceView.Refresh());

            FollowTail
                .Subscribe(_ => ScrollToEnd());

            ItemsSourceView
                .ToObservable()
                .Subscribe(_ => ScrollToEnd());

            SelectedFiles
                .ToObservable()
                .Subscribe(e =>
                {
                    _selection.Sync(e);
                    ItemsSourceView.Refresh();
                });

            SearchRegex.Subscribe(pattern => FindNext(pattern, -1));
        }

        private void ScrollToEnd()
        {
            if (FollowTail.Value &&
                ItemsSourceView.MoveCurrentToLast())
            {
                ScrollToItem.Value = (LogEntry)ItemsSourceView.CurrentItem;
                ScrollToIndex.Value = ItemsSourceView.CurrentPosition;
            }
        }

        public void AddFileToFilter(string fileName)
        {
            if (!AllFiles.Contains(fileName))
            {
                AllFiles.Add(fileName);
                SelectedFiles.Add(fileName);
            }
        }

        private void FindNext(string pattern, int startIndex) =>
            ScrollIntoView(pattern, startIndex, ListSortDirection.Ascending);

        private void FindPrev(string pattern, int startIndex) =>
            ScrollIntoView(pattern, startIndex, ListSortDirection.Descending);

        private void ScrollIntoView(string pattern, int startIndex, ListSortDirection direction)
        {
            FollowTail.Value = false;

            var regex = RegexCache.GetRegex(pattern);

            var result = direction == ListSortDirection.Ascending
                ? ItemsAndIndexes.Skip(startIndex + 1).FirstOrDefault(x => regex.IsMatch(x.Item.Message))
                : ItemsAndIndexes.Take(startIndex).LastOrDefault(x => regex.IsMatch(x.Item.Message));

            if (result.Item != null) // match found
            {
                ScrollToItem.Value = result.Item;
                ScrollToIndex.Value = result.Index;
            }
        }

        private bool Filter(object o)
        {
            return o is LogEntry log && FilterByLevel(log) && FilterByInclude(log) && FilterByExclude(log) && FilterByFile(log);

            bool FilterByLevel(LogEntry log) =>
                ShowErrors.Value && log.Level == LogLevel.ERROR ||
                ShowWarnings.Value && log.Level == LogLevel.WARN ||
                ShowInfos.Value && log.Level == LogLevel.INFO ||
                ShowNotices.Value && log.Level == LogLevel.NOTICE;

            bool FilterByInclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(IncludeRegex.Value) || RegexCache.GetRegex(IncludeRegex.Value).IsMatch(log.Message);

            bool FilterByExclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(ExcludeRegex.Value) || !RegexCache.GetRegex(ExcludeRegex.Value).IsMatch(log.Message);

            bool FilterByFile(LogEntry log) =>
                AllFiles.Count == 0 || _selection.Contains(log.FileName);
        }
    }
}
