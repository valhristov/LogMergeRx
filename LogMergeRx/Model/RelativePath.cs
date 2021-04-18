using System;
using System.Diagnostics;
using System.IO;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("RelativePath:{Value}")]
    public struct RelativePath : IEquatable<RelativePath?>
    {
        private readonly string _value;

        private RelativePath(string path)
        {
            _value = path;
        }

        public static implicit operator string(RelativePath filePath) =>
            filePath._value;

        public static RelativePath FromPathAndRoot(AbsolutePath root, string path) =>
            new RelativePath(Path.GetRelativePath(root, path));

        public static RelativePath FromPath(string fileName) =>
            new RelativePath(fileName);

        public AbsolutePath ToAbsolute(AbsolutePath root) =>
            (AbsolutePath)(Path.Combine(root, _value));

        public bool Equals(RelativePath? other) =>
            _value != null && _value.Equals(other?._value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            Equals(obj as RelativePath?);

        public override int GetHashCode() =>
            _value == null ? 0 : _value.GetHashCode();

        public override string ToString() =>
            _value;

        public static bool operator ==(RelativePath left, RelativePath right) =>
            left.Equals(right);

        public static bool operator !=(RelativePath left, RelativePath right) =>
            !(left == right);
    }
}
