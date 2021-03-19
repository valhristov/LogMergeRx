using System;

namespace LogMergeRx.Model
{
    public struct AbsolutePath : IEquatable<AbsolutePath?>
    {
        public string Value { get; }

        private AbsolutePath(string path)
        {
            Value = path;
        }

        public static implicit operator string(AbsolutePath filePath) =>
            filePath.Value;

        public static AbsolutePath FromFullPath(string fullPath) =>
            new AbsolutePath(fullPath);

        public bool Equals(AbsolutePath? other) =>
            Value.Equals(other?.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            Equals(obj as AbsolutePath?);

        public override int GetHashCode() =>
            Value == null ? 0 : Value.GetHashCode();

        public override string ToString() =>
            Value;

        public static bool operator ==(AbsolutePath left, AbsolutePath right) =>
            left.Equals(right);

        public static bool operator !=(AbsolutePath left, AbsolutePath right) =>
            !(left == right);
    }
}
