using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LogMergeRx
{
    public static class Meter
    {
        public static IDisposable MeasureBegin([CallerMemberName] string callerMemberName = null) =>
            new MeterResult(callerMemberName);

        private class MeterResult : IDisposable
        {
            private readonly string _memberName;
            private readonly string _argument;
            private readonly Stopwatch _stopwatch;

            public MeterResult(string memberName, string argument = null)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _memberName = memberName;
                _argument = argument;
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                Debug.WriteLine($"{_memberName} {_argument} took {_stopwatch.ElapsedMilliseconds}ms.");
            }
        }
    }
}
