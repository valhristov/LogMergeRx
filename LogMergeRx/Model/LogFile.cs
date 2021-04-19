using System;
using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("LogFile:{Id},{Path}")]
    public struct LogFile : IEquatable<LogFile>
    {
        public FileId Id { get; }
        public RelativePath Path { get; }

        public LogFile(FileId fileId, RelativePath relativePath)
        {
            Id = fileId;
            Path = relativePath;
        }

        public override bool Equals(object obj) =>
            obj is LogFile other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Id, Path);

        public bool Equals(LogFile other) =>
            Id == other.Id;

        public static bool operator ==(LogFile left, LogFile right) =>
            left.Equals(right);

        public static bool operator !=(LogFile left, LogFile right) =>
            !(left == right);

        public override string ToString() =>
            Id.ToString();
    }
}
