using System;
using System.Diagnostics;

namespace LogMergeRx
{
    [DebuggerDisplay("FileId:{Id}")]
    public struct FileId : IEquatable<FileId>
    {
        public int Id { get; }

        public FileId(int id)
        {
            Id = id;
        }

        public override bool Equals(object obj) =>
            obj is FileId other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Id);

        public bool Equals(FileId other) =>
            Id == other.Id;

        public static bool operator ==(FileId left, FileId right) =>
            left.Equals(right);

        public static bool operator !=(FileId left, FileId right) =>
            !(left == right);
    }
}
