﻿using LogMergeRx.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;

namespace LogMergeRx.LogViewer
{
    public class LogViewerViewModel
    {
        public WpfObservableRangeCollection<LogEntry> ItemsSource { get; } =
            new WpfObservableRangeCollection<LogEntry>();

        public string FileName { get; }
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

        public LogViewerViewModel(string fileName)
        {
            FileName = fileName;

            ShowErrors = new ObservableProperty<bool>(true);
            ShowWarnings = new ObservableProperty<bool>(true);
            ShowNotices = new ObservableProperty<bool>(true);
            ShowInfos = new ObservableProperty<bool>(true);
            IncludeRegex = new ObservableProperty<string>(string.Empty);
            ExcludeRegex = new ObservableProperty<string>(string.Empty);
            SearchRegex = new ObservableProperty<string>(string.Empty);
            ScrollToIndex = new ObservableProperty<int>(0);

            NextIndex = new ActionCommand(_ => FindNext(SearchRegex.Value, ScrollToIndex.Value));
            PrevIndex = new ActionCommand(_ => FindPrev(SearchRegex.Value, ScrollToIndex.Value));

            ItemsSourceView.Filter = Filter;

            Observable
                .Merge(ShowErrors, ShowWarnings, ShowNotices, ShowInfos)
                .Subscribe(_ => ItemsSourceView.Refresh());
            Observable
                .Merge(IncludeRegex, ExcludeRegex)
                .Subscribe(_ => ItemsSourceView.Refresh());

            SearchRegex.Subscribe(pattern => FindNext(pattern, -1));
        }

        private void FindNext(string pattern, int startIndex) =>
            ScrollIntoView(pattern, startIndex, ListSortDirection.Ascending);

        private void FindPrev(string pattern, int startIndex) =>
            ScrollIntoView(pattern, startIndex, ListSortDirection.Descending);

        private void ScrollIntoView(string pattern, int startIndex, ListSortDirection direction)
        {
            var regex = RegexCache.GetRegex(pattern);

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
                ShowErrors.Value && "ERROR".Equals(log.Level, StringComparison.OrdinalIgnoreCase) ||
                ShowWarnings.Value && "WARN".Equals(log.Level, StringComparison.OrdinalIgnoreCase) ||
                ShowInfos.Value && "INFO".Equals(log.Level, StringComparison.OrdinalIgnoreCase) ||
                ShowNotices.Value && "NOTICE".Equals(log.Level, StringComparison.OrdinalIgnoreCase);

            bool FilterByInclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(IncludeRegex.Value) || RegexCache.GetRegex(IncludeRegex.Value).IsMatch(log.Message);

            bool FilterByExclude(LogEntry log) =>
                string.IsNullOrWhiteSpace(ExcludeRegex.Value) || !RegexCache.GetRegex(ExcludeRegex.Value).IsMatch(log.Message);
        }
    }
}
