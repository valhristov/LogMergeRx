using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogMergeRx
{
    public static class RegexCache
    {
        private static ConcurrentDictionary<string, Lazy<Regex>> _regexCache =
            new ConcurrentDictionary<string, Lazy<Regex>>();

        public static Regex GetRegex(string pattern) =>
            _regexCache.GetOrAdd(pattern, CreateLazyRegex).Value;

        private static Lazy<Regex> CreateLazyRegex(string pattern) =>
            new Lazy<Regex>(() => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }
}
