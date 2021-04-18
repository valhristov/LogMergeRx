using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

namespace LogMergeRx
{
    public static class ListExtensions
    {
        public static void Sync<T>(this ISet<T> list, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (T item in args.OldItems)
                {
                    list.Remove(item);
                }
            }
            if (args.NewItems != null)
            {
                foreach (T item in args.NewItems)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }
        }

        public static void Sync<T>(this IList<T> list, SelectionChangedEventArgs args)
        {
            foreach (var item in args.RemovedItems.OfType<T>())
            {
                list.Remove(item);
            }
            foreach (var item in args.AddedItems.OfType<T>().Where(x => !list.Contains(x)))
            {
                list.Add(item);
            }
        }

        public static void Sync(this IList list, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (var item in args.OldItems)
                {
                    list.Remove(item);
                }
            }
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }
        }

    }
}
