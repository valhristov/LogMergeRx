using System;
using System.Collections.Generic;
using System.Reactive;
using LogMergeRx.Model;

namespace LogMergeRx.ViewModels
{
    public class RegexViewModel : IFilterViewModel
    {
        private readonly bool negateFilter;

        public ObservableProperty<string> RegexString { get; } = new ObservableProperty<string>(string.Empty);
        public ActionCommand ClearCommand { get; }
        public IObservable<Unit> FilterChanges { get; }

        public RegexViewModel(bool negateFilter)
        {
            ClearCommand = new ActionCommand(_ => Clear(), _ => IsFiltered());
            ClearCommand.UpdateCanExecuteOn(RegexString);

            FilterChanges = RegexString.ToUnit();
            this.negateFilter = negateFilter;
        }

        public bool IsFiltered() => !RegexString.IsInitial;

        public void Clear() => RegexString.Reset();

        public IEnumerable<string> GetFilterValues()
        {
            if (IsFiltered()) yield return $"{(negateFilter ? "not matching" : "matching")} '{RegexString.Value}'";
        }

        private bool ApplyNegation(bool filterResult) =>
            negateFilter ? !filterResult : filterResult;

        public bool Filter(LogEntry log) =>
            string.IsNullOrWhiteSpace(RegexString.Value) || ApplyNegation(RegexCache.GetRegex(RegexString.Value).IsMatch(log.Message));
    }
}
