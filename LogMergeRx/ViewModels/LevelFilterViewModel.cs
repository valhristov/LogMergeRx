using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using LogMergeRx.Model;

namespace LogMergeRx.ViewModels
{
    public class LevelFilterViewModel : IFilterViewModel
    {
        public ObservableProperty<bool> ShowErrors { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowWarnings { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowNotices { get; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> ShowInfos { get; } = new ObservableProperty<bool>(true);
        public IObservable<Unit> FilterChanges { get; }

        public LevelFilterViewModel()
        {
            FilterChanges = Observable
                .Merge(ShowErrors, ShowWarnings, ShowNotices, ShowInfos)
                .ToUnit();
        }
        public void Clear()
        {
            ShowErrors.Reset();
            ShowWarnings.Reset();
            ShowNotices.Reset();
            ShowInfos.Reset();
        }

        public bool Filter(LogEntry log) =>
            ShowErrors.Value && log.Level == LogLevel.ERROR ||
            ShowWarnings.Value && log.Level == LogLevel.WARN ||
            ShowInfos.Value && log.Level == LogLevel.INFO ||
            ShowNotices.Value && log.Level == LogLevel.NOTICE||
            log.Level == LogLevel.UNKNOWN;

        public IEnumerable<string> GetFilterValues()
        {
            if (!ShowErrors.Value) yield return "no errors";
            if (!ShowWarnings.Value) yield return "no warnings";
            if (!ShowInfos.Value) yield return "no infos";
            if (!ShowNotices.Value) yield return "no notices";
        }

        public bool IsFiltered() =>
            !ShowErrors.IsInitial ||
            !ShowWarnings.IsInitial ||
            !ShowInfos.IsInitial ||
            !ShowNotices.IsInitial;
    }
}
