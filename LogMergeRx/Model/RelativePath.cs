using System;
using System.IO;

namespace LogMergeRx.Model
{
    public struct RelativePath : IEquatable<RelativePath?>
    {
        public string Value { get; }

        private RelativePath(string path)
        {
            Value = path;
        }

        public static implicit operator string(RelativePath filePath) =>
            filePath.Value;

        public static RelativePath FromPathAndRoot(AbsolutePath root, string path) =>
            new RelativePath(Path.GetRelativePath(root, path));

        public static RelativePath FromPath(string fileName) =>
            new RelativePath(fileName);

        public AbsolutePath ToAbsolute(AbsolutePath root) =>
            AbsolutePath.FromFullPath(Path.Combine(root, Value));

        public bool Equals(RelativePath? other) =>
            Value.Equals(other?.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            Equals(obj as RelativePath?);

        public override int GetHashCode() =>
            Value == null ? 0 : Value.GetHashCode();

        public override string ToString() =>
            Value;

        public static bool operator ==(RelativePath left, RelativePath right) =>
            left.Equals(right);

        public static bool operator !=(RelativePath left, RelativePath right) =>
            !(left == right);
    }
}
