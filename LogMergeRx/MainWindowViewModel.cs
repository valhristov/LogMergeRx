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
        private readonly HashSet<int> _selection = new HashSet<int>();

        public WpfObservableRangeCollection<LogEntry> ItemsSource { get; } =
            new WpfObservableRangeCollection<LogEntry>();

        public ObservableProperty<bool> FollowTail { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowErrors { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowWarnings { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowNotices { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowInfos { get; } = new ObservableProperty<bool>(true);

        public ObservableProperty<string> IncludeRegex { get; } = new ObservableProperty<string>(string.Empty);
        public ObservableProperty<string> ExcludeRegex { get; } = new ObservableProperty<string>(string.Empty);
        public ObservableProperty<string> SearchRegex { get; } = new ObservableProperty<string>(string.Empty);

        public ObservableProperty<int> ScrollToIndex { get; } = new ObservableProperty<int>(0);
        public ObservableProperty<LogEntry> ScrollToItem { get; } = new ObservableProperty<LogEntry>(null);

        public ObservableProperty<DateTime> MinDate { get; } = new ObservableProperty<DateTime>(DateTime.MinValue);

        public ActionCommand NextIndex { get; }
        public ActionCommand PrevIndex { get; }
        public ActionCommand ShowNewerThanNow { get; }

        private ListCollectionView ItemsSourceView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(ItemsSource);

        private IEnumerable<(int Index, LogEntry Item)> ItemsAndIndexes =>
            ItemsSourceView.Cast<LogEntry>().Select((item, index) => (index, item));

        public void UpdateFileName(LogFile logFile)
        {
            var fileViewModel = AllFiles.FirstOrDefault(vm => vm.FileId == logFile.Id);
            if (fileViewModel != null)
            {
                fileViewModel.RelativePath.Value = logFile.Path;
            }
        }

        public WpfObservableRangeCollection<FileViewModel> AllFiles { get; } =
            new WpfObservableRangeCollection<FileViewModel>();

        public WpfObservableRangeCollection<FileViewModel> SelectedFiles { get; } =
            new WpfObservableRangeCollection<FileViewModel>();

        public MainWindowViewModel()
        {
            NextIndex = new ActionCommand(_ => FindNext(SearchRegex.Value, ScrollToIndex.Value));
            PrevIndex = new ActionCommand(_ => FindPrev(SearchRegex.Value, ScrollToIndex.Value));

            ShowNewerThanNow = new ActionCommand(param => MinDate.Value = param is bool enabled && enabled ? DateTime.Now : DateTime.MinValue);

            ItemsSourceView.Filter = Filter;
            ItemsSourceView.CustomSort = new LogEntryDateComparer();

            Observable
                .Merge(ShowErrors, ShowWarnings, ShowNotices, ShowInfos)
                .Subscribe(_ => ItemsSourceView.Refresh());
            Observable
                .Merge(IncludeRegex, ExcludeRegex)
                .Subscribe(_ => ItemsSourceView.Refresh());
            Observable
                .Merge(MinDate)
                .Subscribe(_ => ItemsSourceView.Refresh());

            FollowTail
                .Where(value => value) // only when enabled
                .Subscribe(_ => ScrollToEnd());

            ItemsSourceView // We scroll to end when new items arrive
                .ToObservable()
                .Where(_ => FollowTail.Value) // only when FollowTail is enabled
                .ObserveOnDispatcher() // not nice, but without this the initial scroll to end does not work
                .Subscribe(_ => ScrollToEnd());

            SelectedFiles
                .ToObservable()
                .Subscribe(e =>
                {
                    _selection.Clear();
                    _selection.UnionWith(SelectedFiles.Select(p => p.FileId.Id));
                    ItemsSourceView.Refresh();
                });

            SearchRegex
                .Subscribe(pattern => FindNext(pattern, -1));
        }

        private void ScrollToEnd()
        {
            if (ItemsSourceView.MoveCurrentToLast())
            {
                ScrollToItem.Value = (LogEntry)ItemsSourceView.CurrentItem;
                ScrollToIndex.Value = ItemsSourceView.CurrentPosition;
            }
        }

        public void AddFileToFilter(LogFile logFile)
        {
            if (!AllFiles.Any(x => x.FileId == logFile.Id))
            {
                var viewModel = new FileViewModel(logFile.Id, logFile.Path);
                AllFiles.Add(viewModel);
                SelectedFiles.Add(viewModel);
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
            return o is LogEntry log
                && FilterByLevel(log)
                && FilterByInclude(log)
                && FilterByExclude(log)
                && FilterByFile(log)
                && FilterByDate(log);

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
                AllFiles.Count == 0 || _selection.Contains(log.FileId.Id);

            bool FilterByDate(LogEntry log) =>
                log.Date > MinDate.Value;
        }
    }
}
