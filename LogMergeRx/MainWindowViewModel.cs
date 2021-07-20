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
        private readonly HashSet<int> _fileFilter = new HashSet<int>();

        public WpfObservableRangeCollection<LogEntry> ItemsSource { get; } =
            new WpfObservableRangeCollection<LogEntry>();

        public DateFilterViewModel DateFilterViewModel { get; } = new DateFilterViewModel();
        public RegexViewModel IncludeRegexViewModel { get; } = new RegexViewModel(negateFilter: false);
        public RegexViewModel ExcludeRegexViewModel { get; } = new RegexViewModel(negateFilter: true);
        public LevelFilterViewModel LevelFilterViewModel { get; } = new LevelFilterViewModel();

        public ObservableProperty<bool> FollowTail { get; } = new ObservableProperty<bool>(true);

        public ObservableProperty<string> SearchRegex { get; } = new ObservableProperty<string>(string.Empty);

        public ObservableProperty<int> ScrollToIndex { get; } = new ObservableProperty<int>(0);
        public ObservableProperty<LogEntry> ScrollToItem { get; } = new ObservableProperty<LogEntry>(null);

        public void AddItems(ImmutableList<LogEntry> items)
        {
            DateFilterViewModel.ItemsAdded(items, ItemsSource.Count == 0);
            ItemsSource.AddRange(items);
        }

        public ReadOnlyObservableProperty<string> FiltersText { get; }

        public ActionCommand Find { get; }
        public ActionCommand NextIndex { get; }
        public ActionCommand PrevIndex { get; }
        public ActionCommand ClearFilter { get; }
        public ActionCommand RefreshItemsSource { get; }
        public ActionCommand ScrollToLast { get; }
        public ActionCommand UpdateFileFilter { get; }
        public ActionCommand ClearSearchRegex { get; }

        private ListCollectionView ItemsSourceView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(ItemsSource);

        private ListCollectionView AllFilesView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(AllFiles);

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

        public MainWindowViewModel(IScheduler scheduler)
        {
            Find = new ActionCommand(_ => FindNext(SearchRegex.Value, -1));
            Find.ExecuteOn(SearchRegex.Throttle(TimeSpan.FromMilliseconds(500), scheduler));

            NextIndex = new ActionCommand(_ => FindNext(SearchRegex.Value, ScrollToIndex.Value));
            PrevIndex = new ActionCommand(_ => FindPrev(SearchRegex.Value, ScrollToIndex.Value));

            // Order loaded files alphabetically
            AllFilesView.CustomSort = new FunctionComparer<FileViewModel>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.RelativePath.Value, y.RelativePath.Value));

            ItemsSourceView.Filter = Filter;
            ItemsSourceView.CustomSort = new FunctionComparer<LogEntry>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Date, y.Date));

            var anyFilterChanged = Observable.Merge(
                LevelFilterViewModel.FilterChanges.ToObject(),
                IncludeRegexViewModel.FilterChanges.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                ExcludeRegexViewModel.FilterChanges.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                DateFilterViewModel.FilterChanges.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                SelectedFiles.ToObservable().ToObject());

            RefreshItemsSource = new ActionCommand(_ => ItemsSourceView.Refresh());
            RefreshItemsSource.ExecuteOn(anyFilterChanged);

            ScrollToLast = new ActionCommand(_ => ScrollToEnd());
            ScrollToLast.ExecuteOn(
                Observable
                    .Merge(FollowTail.ToObject(), ItemsSourceView.ToObservable())
                    .Where(_ => FollowTail.Value) // only when follow tail is checked
                    .ObserveOn(scheduler));

            FiltersText = new ReadOnlyObservableProperty<string>(anyFilterChanged.Select(_ => GetFiltersText()));

            UpdateFileFilter = new ActionCommand(
                _ =>
                {
                    _fileFilter.Clear();
                    _fileFilter.UnionWith(SelectedFiles.Select(p => p.FileId.Id));

                    ItemsSourceView.Refresh(); // HACK we need to update the items source view after we update the file filter
                });
            UpdateFileFilter.ExecuteOn(SelectedFiles.ToObservable());

            ClearSearchRegex = new ActionCommand(_ => SearchRegex.Reset(), _ => !SearchRegex.IsInitial);
            ClearSearchRegex.UpdateCanExecuteOn(SearchRegex);

            ClearFilter = new ActionCommand(ClearFilters, HasFilters);
            ClearFilter.UpdateCanExecuteOn(anyFilterChanged);
        }

        private string GetFiltersText() =>
            string.Join(", ", Enumerable.Empty<string>()
                    .Union(LevelFilterViewModel.GetFilterValues())
                    .Union(DateFilterViewModel.GetFilterValues())
                    .Union(ExcludeRegexViewModel.GetFilterValues())
                    .Union(IncludeRegexViewModel.GetFilterValues()));

        private void ClearFilters(object _)
        {
            LevelFilterViewModel.Clear();
            IncludeRegexViewModel.Clear();
            ExcludeRegexViewModel.Clear();
            DateFilterViewModel.Clear();
            _fileFilter.UnionWith(AllFiles.Select(x => x.FileId.Id));
        }

        private bool HasFilters(object _) =>
            LevelFilterViewModel.IsFiltered() ||
            IncludeRegexViewModel.IsFiltered() ||
            ExcludeRegexViewModel.IsFiltered() ||
            DateFilterViewModel.IsFiltered() ||
            AllFiles.Count != _fileFilter.Count;

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
                && LevelFilterViewModel.Filter(log)
                && IncludeRegexViewModel.Filter(log)
                && ExcludeRegexViewModel.Filter(log)
                && FilterByFile(log)
                && DateFilterViewModel.Filter(log)
                ;

            bool FilterByFile(LogEntry log) =>
                AllFiles.Count == 0 || _fileFilter.Contains(log.FileId.Id);
        }
    }
}
