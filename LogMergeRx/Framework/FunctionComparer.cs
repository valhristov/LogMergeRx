using System;
using System.Collections;
using System.Collections.Generic;

namespace LogMergeRx
{
    public class FunctionComparer<T> : IComparer, IComparer<T>
    {
        private readonly Func<T, T, int> _compare;

        public FunctionComparer(Func<T, T, int> compare)
        {
            _compare = compare;
        }

        public int Compare(object x, object y) =>
            x is T xentry &&
            y is T yentry
                ? Compare(xentry, yentry)
                : 0;

        public int Compare(T x, T y) =>
            _compare(x, y);
    }
}