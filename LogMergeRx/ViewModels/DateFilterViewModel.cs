using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class DateFilterViewModel : IFilterViewModel
    {
        public ObservableProperty<double> Minimum { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MinValue));
        public ObservableProperty<double> Start { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MinValue));
        public ObservableProperty<double> End { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MaxValue));
        public ObservableProperty<double> Maximum { get; } = new ObservableProperty<double>(DateTimeHelper.FromDateToSeconds(DateTime.MaxValue));

        public ReadOnlyObservableProperty<DateTime> StartDate { get; }
        public ReadOnlyObservableProperty<DateTime> EndDate { get; }

        public ActionCommand ClearCommand { get; }
        public ActionCommand ShowNewerThanNowCommand { get; }

        public bool Filter(LogEntry log) =>
            log.Date >= StartDate.Value &&
            log.Date <= EndDate.Value;

        public void ItemsAdded(ImmutableList<LogEntry> items, bool theseAreTheFirstItems)
        {
            if (items.Count == 0)
            {
                return;
            }

            if (theseAreTheFirstItems)
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
        }

        public IObservable<Unit> FilterChanges { get; }

        public DateFilterViewModel()
        {
            StartDate = new ReadOnlyObservableProperty<DateTime>(Start.Select(DateTimeHelper.FromSecondsToDate), DateTime.MinValue);
            EndDate = new ReadOnlyObservableProperty<DateTime>(End.Select(DateTimeHelper.FromSecondsToDate), DateTime.MaxValue);

            FilterChanges = Start.Merge(End).ToUnit();

            ClearCommand = new ActionCommand(_ => Clear(), _ => IsFiltered());
            ClearCommand.UpdateCanExecuteOn(FilterChanges.ToObject());

            ShowNewerThanNowCommand = new ActionCommand(_ => Start.Value = DateTimeHelper.FromDateToSeconds(DateTime.Now));
        }

        public void SetStartEnd(LogEntry entry, object parameter)
        {
            if (entry == null || parameter is not TimeSpan ts) return;

            var entrySeconds = DateTimeHelper.FromDateToSeconds(entry.Date);
            Start.Value = Math.Max(Minimum.Value, entrySeconds - ts.TotalSeconds);
            End.Value = Math.Min(Maximum.Value, entrySeconds + ts.TotalSeconds);
        }

        public bool IsFiltered() =>
            !Start.IsInitial || !End.IsInitial;

        public void Clear()
        {
            Start.Value = Minimum.Value;
            End.Value = Maximum.Value;
        }

        public IEnumerable<string> GetFilterValues()
        {
            if (Start.Value != Minimum.Value) yield return $"older than {StartDate.Value:f}";
            if (End.Value != Maximum.Value) yield return $"newer than {EndDate.Value:f}";
        }
    }
}
