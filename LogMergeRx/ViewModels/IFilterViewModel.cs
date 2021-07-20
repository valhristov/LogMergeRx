using System.Collections.Generic;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public interface IFilterViewModel
    {
        bool Filter(LogEntry log);
        bool IsFiltered();
        void Clear();
        IEnumerable<string> GetFilterValues();
    }
}
