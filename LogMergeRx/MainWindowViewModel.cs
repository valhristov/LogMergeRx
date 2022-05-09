using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Data;
using LogMergeRx.Model;
using LogMergeRx.ViewModels;

namespace LogMergeRx
{
    public class MainWindowViewModel
    {
        public WpfObservableRangeCollection<LogEntry> ItemsSource { get; } =
            new WpfObservableRangeCollection<LogEntry>();

        public DateFilterViewModel DateFilterViewModel { get; } = new DateFilterViewModel();
        public RegexViewModel IncludeRegexViewModel { get; } = new RegexViewModel(negateFilter: false);
        public RegexViewModel ExcludeRegexViewModel { get; } = new RegexViewModel(negateFilter: true);
        public LevelFilterViewModel LevelFilterViewModel { get; } = new LevelFilterViewModel();
        public FileFilterViewModel FileFilterViewModel { get; } = new FileFilterViewModel();
        public SourceFilterViewModel SourceFilterViewModel { get; } = new SourceFilterViewModel();

        public ObservableProperty<bool> FollowTail { get; } = new ObservableProperty<bool>(true);

        public ObservableProperty<string> SearchRegex { get; } = new ObservableProperty<string>(string.Empty);

        public ObservableProperty<int> ScrollToIndex { get; } = new ObservableProperty<int>(0);
        public ObservableProperty<LogEntry> ScrollToItem { get; } = new ObservableProperty<LogEntry>(null);

        public void AddItems(ImmutableList<LogEntry> items)
        {
            DateFilterViewModel.ItemsAdded(items, ItemsSource.Count == 0);
            ItemsSource.AddRange(items);
            SourceFilterViewModel.AddSourcesToFilter(items.Select(x => x.Source));
        }

        public ReadOnlyObservableProperty<string> FiltersText { get; }

        public ActionCommand Find { get; }
        public ActionCommand NextIndex { get; }
        public ActionCommand PrevIndex { get; }
        public ActionCommand ClearFilter { get; }
        public ActionCommand ScrollToLast { get; }
        public ActionCommand ClearSearchRegex { get; }

        private ListCollectionView ItemsSourceView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(ItemsSource);

        private IEnumerable<(int Index, LogEntry Item)> ItemsAndIndexes =>
            ItemsSourceView.Cast<LogEntry>().Select((item, index) => (index, item));

        public void UpdateFileName(LogFile logFile) => FileFilterViewModel.UpdateFileName(logFile);

        private IEnumerable<IFilterViewModel> Filters { get; }

        public MainWindowViewModel(IScheduler scheduler)
        {
            Filters = ImmutableArray.Create<IFilterViewModel>(
                LevelFilterViewModel,
                IncludeRegexViewModel,
                ExcludeRegexViewModel,
                DateFilterViewModel,
                FileFilterViewModel,
                SourceFilterViewModel);

            Find = new ActionCommand(_ => FindNext(SearchRegex.Value, -1));
            Find.ExecuteOn(SearchRegex.Throttle(TimeSpan.FromMilliseconds(500), scheduler));

            NextIndex = new ActionCommand(_ => FindNext(SearchRegex.Value, ScrollToIndex.Value));
            PrevIndex = new ActionCommand(_ => FindPrev(SearchRegex.Value, ScrollToIndex.Value));

            ItemsSourceView.Filter = Filter;
            ItemsSourceView.CustomSort = new FunctionComparer<LogEntry>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Date, y.Date));

            var anyFilterChanged = Observable.Merge(
                LevelFilterViewModel.FilterChanges.ToObject(),
                IncludeRegexViewModel.FilterChanges.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                ExcludeRegexViewModel.FilterChanges.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                DateFilterViewModel.FilterChanges.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                FileFilterViewModel.FilterChanges.ToObject(),
                SourceFilterViewModel.FilterChanges.ToObject());

            anyFilterChanged.Subscribe(_ => ItemsSourceView.Refresh());

            Observable
                .Merge(FollowTail.ToObject(), ItemsSourceView.ToObservable())
                .Where(_ => FollowTail.Value) // only when follow tail is checked
                .ObserveOn(scheduler)
                .Subscribe(_ => ScrollToEnd());

            FiltersText = new ReadOnlyObservableProperty<string>(anyFilterChanged.Select(_ => GetFiltersText()));

            ClearSearchRegex = new ActionCommand(_ => SearchRegex.Reset(), _ => !SearchRegex.IsInitial);
            ClearSearchRegex.UpdateCanExecuteOn(SearchRegex);

            ClearFilter = new ActionCommand(ClearFilters, HasFilters);
            ClearFilter.UpdateCanExecuteOn(anyFilterChanged);
        }

        private string GetFiltersText() =>
            string.Join(", ", Filters.SelectMany(f => f.GetFilterValues()));

        private void ClearFilters(object _)
        {
            foreach (var filter in Filters)
	        {
                filter.Clear();
	        }
        }

        private bool HasFilters(object _) =>
            Filters.Any(f => f.IsFiltered());

        private void ScrollToEnd()
        {
            if (ItemsSourceView.MoveCurrentToLast())
            {
                ScrollToItem.Value = (LogEntry)ItemsSourceView.CurrentItem;
                ScrollToIndex.Value = ItemsSourceView.CurrentPosition;
            }
        }

        public void AddFileToFilter(LogFile logFile) =>
            FileFilterViewModel.AddFileToFilter(logFile);

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

        private bool Filter(object o) =>
            o is LogEntry log
            && Filters.All(f => f.Filter(log));
    }
}
