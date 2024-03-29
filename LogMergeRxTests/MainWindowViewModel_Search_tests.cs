﻿using System;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class MainWindowViewModel_Search_tests
    {
        private readonly TestScheduler _scheduler;
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModel_Search_tests()
        {
            _scheduler = new TestScheduler();
            _viewModel = new MainWindowViewModel(_scheduler);
            _viewModel.FollowTail.Value = false;
            _viewModel.AddItems(
                ImmutableArray.Create(
                // only errors are enabled by default
                LogHelper.Create("1", LogLevel.ERROR), // index:0
                LogHelper.Create("1", LogLevel.ERROR), // index:1
                LogHelper.Create("1", LogLevel.ERROR), // index:2
                LogHelper.Create("2", LogLevel.WARN), //  index:3  // index:0 with removed errors
                LogHelper.Create("3", LogLevel.ERROR), // index:4
                LogHelper.Create("2", LogLevel.WARN), //  index:5  // index:1 with removed errors
                LogHelper.Create("2", LogLevel.WARN), //  index:6  // index:2 with removed errors
                LogHelper.Create("3", LogLevel.WARN), //  index:7  // index:3 with removed errors
                LogHelper.Create("2", LogLevel.WARN), //  index:8  // index:4 with removed errors
                LogHelper.Create("2", LogLevel.WARN), //  index:9  // index:5 with removed errors
                LogHelper.Create("1", LogLevel.WARN)  //  index:10 // index:6 with removed errors
                ));
        }

        private static readonly TimeSpan DefaultThrottle = TimeSpan.FromMilliseconds(510);

        private void DoAndWait(Action action)
        {
            action();
            _scheduler.AdvanceBy(DefaultThrottle.Ticks);
            DispatcherUtil.DoEvents();
        }

        [TestMethod]
        public void SearchRegex_always_searches_from_start()
        {
            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            DoAndWait(() => _viewModel.SearchRegex.Value = "1");
            _viewModel.ScrollToIndex.Value.Should().Be(0);

            DoAndWait(() => _viewModel.SearchRegex.Value = "2");
            _viewModel.ScrollToIndex.Value.Should().Be(3);

            DoAndWait(() => _viewModel.SearchRegex.Value = "3");
            _viewModel.ScrollToIndex.Value.Should().Be(4);

            DoAndWait(() => _viewModel.SearchRegex.Value = "1");
            _viewModel.ScrollToIndex.Value.Should().Be(0);
        }

        [TestMethod]
        public void NextIndex_goes_forward()
        {
            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            DoAndWait(() => _viewModel.SearchRegex.Value = "2");
            _viewModel.ScrollToIndex.Value.Should().Be(3);

            DoAndWait(() => _viewModel.NextIndex.Execute(null));
            _viewModel.ScrollToIndex.Value.Should().Be(5);

            DoAndWait(() => _viewModel.NextIndex.Execute(null));
            _viewModel.ScrollToIndex.Value.Should().Be(6);

            DoAndWait(() => _viewModel.NextIndex.Execute(null));
            _viewModel.ScrollToIndex.Value.Should().Be(8);

            DoAndWait(() => _viewModel.NextIndex.Execute(null));
            _viewModel.ScrollToIndex.Value.Should().Be(9);

            DoAndWait(() => _viewModel.NextIndex.Execute(null)); // cannot find more, stay on the last
            _viewModel.ScrollToIndex.Value.Should().Be(9);
        }

        [TestMethod]
        public void PrevIndex_goes_backward()
        {
            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            _viewModel.SearchRegex.Value = "2";
            _viewModel.ScrollToIndex.Value = 10; // position at the end

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(9);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(8);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(6);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(5);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(3);

            _viewModel.PrevIndex.Execute(null); // cannot find more, stay on last result
            _viewModel.ScrollToIndex.Value.Should().Be(3);
        }

        [TestMethod]
        public void Searching_with_filter()
        {
            _viewModel.LevelFilterViewModel.ShowErrors.Value = false; // remove errors, the indexes will shift
            DispatcherUtil.DoEvents();

            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            DoAndWait(() => _viewModel.SearchRegex.Value = "2");
            _viewModel.ScrollToIndex.Value = 0; // first result

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(1);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(2);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(4);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(5);

            _viewModel.NextIndex.Execute(null); // cannot find more, stay on last result
            _viewModel.ScrollToIndex.Value.Should().Be(5);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(4);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(2);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(1);

            _viewModel.PrevIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(0);

            _viewModel.PrevIndex.Execute(null); // cannot find more, stay on last result
            _viewModel.ScrollToIndex.Value.Should().Be(0);
        }
    }
}
