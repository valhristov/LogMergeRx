using LogMergeRx.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Data;

namespace LogMergeRx.ViewModels
{
    public record SourceViewModel(string Name);

    public class SourceFilterViewModel : IFilterViewModel
    {
        private HashSet<string> _selectedSources = new HashSet<string>();

        public WpfObservableRangeCollection<SourceViewModel> AllSources { get; } =
            new WpfObservableRangeCollection<SourceViewModel>();

        public WpfObservableRangeCollection<SourceViewModel> SelectedSources { get; } =
            new WpfObservableRangeCollection<SourceViewModel>();

        private ListCollectionView AllSourcesView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(AllSources);


        public IObservable<Unit> FilterChanges { get; }

        public SourceFilterViewModel()
        {
            SelectedSources.ToObservable().Subscribe(_ =>
            {
                _selectedSources.Clear();
                _selectedSources.UnionWith(SelectedSources.Select(p => p.Name));
            });
            FilterChanges = SelectedSources.ToObservable().ToUnit();

            // Order loaded sources alphabetically
            AllSourcesView.CustomSort = new FunctionComparer<SourceViewModel>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));
        }

        public void AddSourcesToFilter(IEnumerable<string> sources)
        {
            var newSources = sources
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Except(AllSources.Select(x => x.Name))
                .Select(s => new SourceViewModel(s))
                .ToList();
            if (newSources.Count > 0)
            {
                newSources.ForEach(AllSources.Add);
                SelectedSources.AddRange(newSources);
            }
        }

        public void Clear() =>
            AllSources.Except(SelectedSources).ToList().ForEach(SelectedSources.Add);

        public bool Filter(LogEntry log) =>
            _selectedSources.Contains(log.Source);

        public IEnumerable<string> GetFilterValues() =>
            Enumerable.Empty<string>();

        public bool IsFiltered() =>
            AllSources.Count != _selectedSources.Count;
    }
}
