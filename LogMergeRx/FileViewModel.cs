using LogMergeRx.Model;

namespace LogMergeRx
{
    public class FileViewModel
    {
        public FileId FileId { get; }
        public ObservableProperty<RelativePath> RelativePath { get; } = new ObservableProperty<RelativePath>();

        public FileViewModel(FileId fileId)
        {
            FileId = fileId;
        }
    }
}
