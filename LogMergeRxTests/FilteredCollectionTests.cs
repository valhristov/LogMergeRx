using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx.Tests
{
    [TestClass]
    public class FilteredCollectionTests
    {
        private const string CollectionChanged = "CollectionChanged";
        [TestMethod]
        public void AddRange_Raises_CollectionChanged()
        {
            var col = new FilteredCollection<Item>();

            var monitor = col.Monitor();

            col.AddRange(new[] { new Item { Value = 1 }, new Item { Value = 2 } });

            monitor.OccurredEvents.Should().HaveCount(2);
            monitor.OccurredEvents.Should().Contain(x => x.EventName == "CollectionChanged");
        }

        [TestMethod]
        public void Filter_Raises_CollectionChanged_1()
        {
            var col = new FilteredCollection<Item>(new[] { new Item { Value = 1 }, new Item { Value = 2 } });

            var monitor = col.Monitor();

            col.Filter.Value = x => x.Value == 1;

            monitor.OccurredEvents.Should().HaveCount(2);
            monitor.OccurredEvents.Where(IsCollectionChanged).Should().HaveCount(1);

            AssertCollectionChanged(monitor.OccurredEvents.First(IsCollectionChanged), newCount: 0, oldCount: 1);
        }

        [TestMethod]
        public void Filter_Raises_CollectionChanged_2()
        {
            var col = new FilteredCollection<Item>(new[] { new Item { Value = 1 }, new Item { Value = 2 } });

            col.Filter.Value = x => x.Value == 1;

            var monitor = col.Monitor();

            col.Filter.Value = x => x.Value == 2;

            monitor.OccurredEvents.Should().HaveCount(2);
            monitor.OccurredEvents.Where(IsCollectionChanged).Should().HaveCount(1);

            AssertCollectionChanged(monitor.OccurredEvents.First(IsCollectionChanged), newCount: 1, oldCount: 1);
        }

        private static bool IsCollectionChanged(OccurredEvent e) =>
            e.EventName == CollectionChanged;

        private static void AssertCollectionChanged(OccurredEvent e, int newCount, int oldCount)
        {
            var args = (NotifyCollectionChangedEventArgs)e.Parameters[1];
            args.NewItems.Should().HaveCount(newCount);
            args.OldItems.Should().HaveCount(oldCount);
        }

        private class Item
        {
            public int Value { get; set; }
        }
    }
}