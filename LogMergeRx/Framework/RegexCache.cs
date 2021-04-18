using System.Text.RegularExpressions;

namespace LogMergeRx
{
    public static class RegexCache
    {
        private static readonly Cache<string, Regex> _regexCache =
            new Cache<string, Regex>(CreateRegex);

        public static Regex GetRegex(string pattern) =>
            _regexCache.Get(pattern);

        private static Regex CreateRegex(string pattern) =>
            new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
