﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
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

        public ObservableProperty<double> Minimum { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MinValue));
        public ObservableProperty<double> Start { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MinValue));
        public ObservableProperty<double> End { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MaxValue));
        public ObservableProperty<double> Maximum { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MaxValue));

        public ReadOnlyObservableProperty<DateTime> StartDate { get; }
        public ReadOnlyObservableProperty<DateTime> EndDate { get; }

        public ReadOnlyObservableProperty<string> FiltersText { get; }

        public ActionCommand NextIndex { get; }
        public ActionCommand PrevIndex { get; }
        public ActionCommand ShowNewerThanNow { get; }
        public ActionCommand ClearFilter { get; }
        public ActionCommand RefreshItemsSource { get; }
        public ActionCommand ScrollToLast { get; }

        private ListCollectionView ItemsSourceView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(ItemsSource);

        public void AddItems(ImmutableList<LogEntry> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            if (ItemsSource.Count == 0)
            {
                Minimum.Value = Start.Value = DateTimeHelper.FromDateToSeconds(items.Min(x => x.Date));
                Maximum.Value = End.Value = DateTimeHelper.FromDateToSeconds(items.Max(x => x.Date));
            }
            else
            {
                var newFirstItem = Math.Min(Minimum.Value, DateTimeHelper.FromDateToSeconds(items.Min(x => x.Date)));
                var newLastItem = Math.Max(Maximum.Value, DateTimeHelper.FromDateToSeconds(items.Max(x => x.Date)));
                if (Start.Value == Minimum.Value)
                { // Move start with minimum when displaying the full range
                    Start.Value = newFirstItem;
                }
                if (End.Value == Maximum.Value)
                { // Move end with maximum when displaying the full range
                    End.Value = newLastItem;
                }

                Minimum.Value = newFirstItem;
                Maximum.Value = newLastItem;
            }

            ItemsSource.AddRange(items);
        }

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
            NextIndex = new ActionCommand(_ => FindNext(SearchRegex.Value, ScrollToIndex.Value));
            PrevIndex = new ActionCommand(_ => FindPrev(SearchRegex.Value, ScrollToIndex.Value));

            ShowNewerThanNow = new ActionCommand(_ => Start.Value = DateTimeHelper.FromDateToSeconds(DateTime.Now));

            // Order loaded files alphabetically
            AllFilesView.CustomSort = new FunctionComparer<FileViewModel>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.RelativePath.Value, y.RelativePath.Value));

            ItemsSourceView.Filter = Filter;
            ItemsSourceView.CustomSort = new FunctionComparer<LogEntry>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Date, y.Date));

            var anyFilterChanged = Observable.Merge(
                ShowErrors.ToObject(),
                ShowWarnings.ToObject(),
                ShowNotices.ToObject(),
                ShowInfos.ToObject(),
                IncludeRegex.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                ExcludeRegex.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                Start.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                End.ToObject().Throttle(TimeSpan.FromMilliseconds(500), scheduler),
                SelectedFiles.ToObservable().ToObject());

            RefreshItemsSource = new ActionCommand(_ => ItemsSourceView.Refresh());
            RefreshItemsSource.Subscribe(anyFilterChanged);

            ScrollToLast = new ActionCommand(_ => ScrollToEnd());
            ScrollToLast.Subscribe(
                Observable
                    .Merge(FollowTail.ToObject(), ItemsSourceView.ToObservable())
                    .Where(_ => FollowTail.Value) // only when follow tail is checked
                    //.Throttle(TimeSpan.FromMilliseconds(100), scheduler)
                    .ObserveOnDispatcher());

            StartDate = new ReadOnlyObservableProperty<DateTime>(Start.Select(DateTimeHelper.FromSecondsToDate), DateTime.MinValue);
            EndDate = new ReadOnlyObservableProperty<DateTime>(End.Select(DateTimeHelper.FromSecondsToDate), DateTime.MaxValue);
            FiltersText = new ReadOnlyObservableProperty<string>(anyFilterChanged.Select(_ => GetFiltersText()));

            SelectedFiles
                .ToObservable()
                .Subscribe(_ =>
                {
                    _selection.Clear();
                    _selection.UnionWith(SelectedFiles.Select(p => p.FileId.Id));
                });

            SearchRegex
                .Throttle(TimeSpan.FromMilliseconds(500), scheduler)
                .Subscribe(pattern => FindNext(pattern, -1));

            ClearFilter = new ActionCommand(ClearFilters, HasFilters);
        }

        private string GetFiltersText()
        {
            return string.Join(", ", GetFilterValues());

            IEnumerable<string> GetFilterValues()
            {
                if (!ShowErrors.Value) yield return "no errors";
                if (!ShowWarnings.Value) yield return "no warnings";
                if (!ShowInfos.Value) yield return "no infos";
                if (!ShowNotices.Value) yield return "no notices";
                if (Start.Value != Minimum.Value) yield return $"older than {StartDate.Value:f}";
                if (End.Value != Maximum.Value) yield return $"newer than {EndDate.Value:f}";
                if (!string.IsNullOrEmpty(IncludeRegex.Value)) yield return $"matching '{IncludeRegex.Value}'";
                if (!string.IsNullOrEmpty(ExcludeRegex.Value)) yield return $"not matching '{ExcludeRegex.Value}'";
            }
        }

        private void ClearFilters(object _)
        {
            ShowErrors.Reset();
            ShowWarnings.Reset();
            ShowInfos.Reset();
            ShowNotices.Reset();
            IncludeRegex.Reset();
            ExcludeRegex.Reset();
            Start.Value = Minimum.Value;
            End.Value = Maximum.Value;
            _selection.UnionWith(AllFiles.Select(x => x.FileId.Id));
        }

        private bool HasFilters(object _) =>
            !(ShowErrors.IsInitial &&
            ShowWarnings.IsInitial &&
            ShowInfos.IsInitial &&
            ShowNotices.IsInitial &&
            IncludeRegex.IsInitial &&
            ExcludeRegex.IsInitial &&
            Start.IsInitial &&
            End.IsInitial &&
            AllFiles.Count == _selection.Count);

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
                && FilterByDate(log)
                ;

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
                log.Date >= StartDate.Value && log.Date <= EndDate.Value;
        }
    }
}
