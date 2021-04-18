using System.Collections.Concurrent;
using System.Threading;
using LogMergeRx.Model;
using Neat.Results;

namespace LogMergeRx
{
    public class FileMap
    {
        private int _lastId = 0;

        private readonly ConcurrentDictionary<RelativePath, FileId> _pathToFileId = new();
        private readonly ConcurrentDictionary<FileId, RelativePath> _fileIdToPath = new();

        public FileId GetOrAddFileId(RelativePath relativePath) =>
            _pathToFileId.GetOrAdd(relativePath,
                key =>
                {
                    var fileId = new FileId(Interlocked.Increment(ref _lastId));
                    _fileIdToPath[fileId] = relativePath;
                    return fileId;
                });

        public Result<RelativePath> GetRelativePath(FileId fileId) =>
            _fileIdToPath.TryGetValue(fileId, out var relativePath)
                ? Result.Success(relativePath)
                : Result.Failure<RelativePath>($"Cannot find the path of file '{fileId}'");

        public Result<FileId> Rename(RelativePath from, RelativePath to) =>
            _pathToFileId.TryRemove(from, out var fileId) &&
            _pathToFileId.TryAdd(to, fileId) &&
            _fileIdToPath.TryUpdate(fileId, to, from)
                ? Result.Success(fileId)
                : Result.Failure<FileId>($"Cannot rename '{from}' to '{to}'");
    }
}
