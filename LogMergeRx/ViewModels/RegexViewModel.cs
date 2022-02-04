using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using LogMergeRx.Model;

namespace LogMergeRx.ViewModels
{
    public class RegexViewModel : IFilterViewModel
    {
        private readonly bool negateFilter;
        private string lastValidRegex;

        public ObservableProperty<string> RegexString { get; } = new ObservableProperty<string>(string.Empty);
        public ActionCommand ClearCommand { get; }
        public IObservable<Unit> FilterChanges { get; }

        public RegexViewModel(bool negateFilter)
        {
            ClearCommand = new ActionCommand(_ => Clear(), _ => IsFiltered());
            ClearCommand.UpdateCanExecuteOn(RegexString);

            var validRegexes = RegexString.Where(IsValidRegex);
            validRegexes.Subscribe(x =>
            {
                Debug.WriteLine(x);
                lastValidRegex = x;
            });

            FilterChanges = validRegexes.ToUnit();
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
            string.IsNullOrWhiteSpace(lastValidRegex) || ApplyNegation(RegexCache.GetRegex(lastValidRegex).IsMatch(log.Message));

        private bool IsValidRegex(string value)
        {
            try
            {
                _ = new Regex(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
