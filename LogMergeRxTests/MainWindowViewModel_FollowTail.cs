using System;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentAssertions;
using LogMergeRx;
using LogMergeRx.Model;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class MainWindowViewModel_FollowTail_Tests
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly TestScheduler _scheduler;

        public MainWindowViewModel_FollowTail_Tests()
        {
            _scheduler = new TestScheduler();
            _viewModel = new MainWindowViewModel(_scheduler);
            _viewModel.AddItems(ImmutableList.Create(
                LogHelper.Create("message error 1", LogLevel.ERROR),
                LogHelper.Create("message error 2", LogLevel.ERROR),
                LogHelper.Create("message warning 1", LogLevel.WARN),
                LogHelper.Create("message warning 2", LogLevel.WARN),
                LogHelper.Create("message notice 1", LogLevel.NOTICE),
                LogHelper.Create("message info 1", LogLevel.INFO)));
        }

        private static readonly TimeSpan DefaultThrottle = TimeSpan.FromMilliseconds(510);

        private void DoAndWait(Action action)
        {
            action();
            _scheduler.AdvanceBy(DefaultThrottle.Ticks);
            DispatcherUtil.DoEvents();
        }

        [TestMethod]
        public void Setting_SearchRegex_Disables_Follow_Tail()
        {
            _viewModel.FollowTail.Value = true;

            DoAndWait(() => _viewModel.SearchRegex.Value = "xxx");

            _viewModel.FollowTail.Value.Should().BeFalse();
        }

        [TestMethod]
        public void FollowTail_True_Adding_New_Item_Scrolls_To_End()
        {
            _viewModel.FollowTail.Value = true;

            var newEntry = LogHelper.Create("new message 1", LogLevel.ERROR);
            _viewModel.AddItems(ImmutableList.Create(newEntry));

            _scheduler.AdvanceBy(10);

            _viewModel.ScrollToItem.Value.Should().Be(newEntry);
            _viewModel.ScrollToIndex.Value.Should().Be(_viewModel.ItemsSource.Count - 1);

            newEntry = LogHelper.Create("new message 2", LogLevel.ERROR);
            _viewModel.AddItems(ImmutableList.Create(newEntry));

            _scheduler.AdvanceBy(10);

            _viewModel.ScrollToItem.Value.Should().Be(newEntry);
            _viewModel.ScrollToIndex.Value.Should().Be(_viewModel.ItemsSource.Count - 1);
        }
    }
}
