using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Data;
using LogMergeRx.Model;

namespace LogMergeRx.ViewModels
{
    public class FileFilterViewModel : IFilterViewModel
    {
        private readonly HashSet<int> _fileFilter = new HashSet<int>();

        public WpfObservableRangeCollection<FileViewModel> AllFiles { get; } =
            new WpfObservableRangeCollection<FileViewModel>();

        public WpfObservableRangeCollection<FileViewModel> SelectedFiles { get; } =
            new WpfObservableRangeCollection<FileViewModel>();

        public ActionCommand UpdateFileFilter { get; }

        public IObservable<Unit> FilterChanges { get; }

        private ListCollectionView AllFilesView =>
            (ListCollectionView)CollectionViewSource.GetDefaultView(AllFiles);

        public FileFilterViewModel()
        {
            SelectedFiles.ToObservable().Subscribe(_ =>
            {
                _fileFilter.Clear();
                _fileFilter.UnionWith(SelectedFiles.Select(p => p.FileId.Id));
            });
            FilterChanges = SelectedFiles.ToObservable().ToUnit();

            // Order loaded files alphabetically
            AllFilesView.CustomSort = new FunctionComparer<FileViewModel>(
                (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.RelativePath.Value, y.RelativePath.Value));
        }

        public void Clear() =>
            AllFiles.Except(SelectedFiles).ToList().ForEach(SelectedFiles.Add);

        public bool Filter(LogEntry log) =>
            AllFiles.Count == 0 || _fileFilter.Contains(log.FileId.Id);

        public IEnumerable<string> GetFilterValues() =>
            Enumerable.Empty<string>();

        public bool IsFiltered() =>
            AllFiles.Count != _fileFilter.Count;

        public void AddFileToFilter(LogFile logFile)
        {
            if (!AllFiles.Any(x => x.FileId == logFile.Id))
            {
                var viewModel = new FileViewModel(logFile.Id, logFile.Path);
                AllFiles.Add(viewModel);
                SelectedFiles.Add(viewModel);
            }
        }

        public void UpdateFileName(LogFile logFile)
        {
            var fileViewModel = AllFiles.FirstOrDefault(vm => vm.FileId == logFile.Id);
            if (fileViewModel != null)
            {
                fileViewModel.RelativePath.Value = logFile.Path;
            }
        }

    }
}
