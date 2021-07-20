using System;
using System.Collections.Generic;
using System.Reactive;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public interface IFilterViewModel
    {
        bool Filter(LogEntry log);
        bool IsFiltered();
        void Clear();
        IEnumerable<string> GetFilterValues();
        IObservable<Unit> FilterChanges { get; }
    }
}
