using System;
using System.IO;

namespace LogMergeRx.Model
{
    public struct FilePath : IEquatable<FilePath?>
    {
        public string FullPath { get; }
        public string Name => Path.GetFileName(FullPath);

        public FilePath(string path)
        {
            FullPath = path;
        }

        public static implicit operator string(FilePath filePath) =>
            filePath.FullPath;

        public static FilePath FromFullPath(string fullPath) =>
            new FilePath(fullPath);

        public bool Equals(FilePath? other) =>
            FullPath.Equals(other?.FullPath, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            Equals(obj as FilePath?);

        public override int GetHashCode() =>
            FullPath == null ? 0 : FullPath.GetHashCode();
    }
}
