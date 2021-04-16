using System.Collections.Concurrent;
using System.Threading;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class FileMap
    {
        private int lastId = 0;

        private readonly ConcurrentDictionary<RelativePath, FileId> pathToFileId = new();
        private readonly ConcurrentDictionary<FileId, RelativePath> fileIdToPath = new();

        public FileId GetOrAddFileId(RelativePath relativePath)
        {
            return pathToFileId.GetOrAdd(relativePath, key =>
            {
                var fileId = new FileId(Interlocked.Increment(ref lastId));
                fileIdToPath[fileId] = relativePath;
                return fileId;
            });
        }

        public bool TryGetRelativePath(FileId fileId, out RelativePath relativePath) =>
            fileIdToPath.TryGetValue(fileId, out relativePath);

        public bool TryRename(RelativePath from, RelativePath to, out FileId fileId) =>
            pathToFileId.TryRemove(from, out fileId) &&
            pathToFileId.TryAdd(to, fileId) &&
            fileIdToPath.TryUpdate(fileId, to, from);
    }
}
