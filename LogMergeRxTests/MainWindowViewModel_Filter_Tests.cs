using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows.Data;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class MainWindowViewModel_Filter_Tests
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly TestScheduler _scheduler;

        public MainWindowViewModel_Filter_Tests()
        {
            _scheduler = new TestScheduler();
            _viewModel = new MainWindowViewModel(_scheduler);
            _viewModel.ItemsSource.Add(LogHelper.Create("message error 1", LogLevel.ERROR, fileId: 0));
            _viewModel.ItemsSource.Add(LogHelper.Create("message error 2", LogLevel.ERROR, fileId: 1));
            _viewModel.ItemsSource.Add(LogHelper.Create("message warning 1", LogLevel.WARN, fileId: 2));
            _viewModel.ItemsSource.Add(LogHelper.Create("message warning 2", LogLevel.WARN, fileId: 0));
            _viewModel.ItemsSource.Add(LogHelper.Create("message notice 1", LogLevel.NOTICE, fileId: 1));
            _viewModel.ItemsSource.Add(LogHelper.Create("message info 1", LogLevel.INFO, fileId: 2));
        }

        private IEnumerable<LogEntry> View =>
            CollectionViewSource.GetDefaultView(_viewModel.ItemsSource).OfType<LogEntry>();

        private static readonly TimeSpan DefaultThrottle = TimeSpan.FromMilliseconds(510);

        private void DoAndWait(Action action)
        {
            action();
            _scheduler.AdvanceBy(DefaultThrottle.Ticks);
        }

        [TestMethod]
        public void Include_errors()
        {
            _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
            _viewModel.LevelFilterViewModel.ShowInfos.Value = false;
            _viewModel.LevelFilterViewModel.ShowNotices.Value = false;

            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message error 1", "message error 2");

            _viewModel.LevelFilterViewModel.ShowErrors.Value = false;

            View.Should().BeEmpty();
        }

        [TestMethod]
        public void Include_warnings()
        {
            _viewModel.LevelFilterViewModel.ShowErrors.Value = false;
            _viewModel.LevelFilterViewModel.ShowInfos.Value = false;
            _viewModel.LevelFilterViewModel.ShowNotices.Value = false;

            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message warning 1", "message warning 2");
        }

        [TestMethod]
        public void Include_notice()
        {
            _viewModel.LevelFilterViewModel.ShowErrors.Value = false;
            _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
            _viewModel.LevelFilterViewModel.ShowInfos.Value = false;

            View.Should().HaveCount(1);
            View.Select(x => x.Message).Should().Equal("message notice 1");
        }

        [TestMethod]
        public void Include_info()
        {
            _viewModel.LevelFilterViewModel.ShowErrors.Value = false;
            _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
            _viewModel.LevelFilterViewModel.ShowNotices.Value = false;

            _viewModel.LevelFilterViewModel.ShowInfos.Value = true;

            View.Should().HaveCount(1);
            View.Select(x => x.Message).Should().Equal("message info 1");
        }

        [TestMethod]
        public void Include_all()
        {
            View.Should().HaveCount(6);
            View.Select(x => x.Message).Should().Equal("message error 1", "message error 2", "message warning 1", "message warning 2", "message notice 1", "message info 1");
        }

        [TestMethod]
        public void Remove_all()
        {
            _viewModel.LevelFilterViewModel.ShowErrors.Value = false; // default
            _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
            _viewModel.LevelFilterViewModel.ShowNotices.Value = false;
            _viewModel.LevelFilterViewModel.ShowInfos.Value = false;

            View.Should().BeEmpty();
        }

        [TestMethod]
        public void Include_regex()
        {
            DoAndWait(() => _viewModel.IncludeRegexViewModel.RegexString.Value = string.Empty); // default
            View.Should().HaveCount(6); // all items

            DoAndWait(() => _viewModel.IncludeRegexViewModel.RegexString.Value = "2");

            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message error 2", "message warning 2");

            // lang=regex
            DoAndWait(() => _viewModel.IncludeRegexViewModel.RegexString.Value = "message.*\\s1");

            View.Should().HaveCount(4);
            View.Select(x => x.Message).Should().Equal("message error 1", "message warning 1", "message notice 1", "message info 1");
        }

        [TestMethod]
        public void Exclude_regex()
        {
            DoAndWait(() => _viewModel.ExcludeRegexViewModel.RegexString.Value = string.Empty); // default
            View.Should().HaveCount(6); // all items

            DoAndWait(() => _viewModel.ExcludeRegexViewModel.RegexString.Value = "2");

            View.Should().HaveCount(4);
            View.Select(x => x.Message).Should().Equal("message error 1", "message warning 1", "message notice 1", "message info 1");

            // lang=regex
            DoAndWait(() => _viewModel.ExcludeRegexViewModel.RegexString.Value = "message.*\\s1");

            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message error 2", "message warning 2");
        }
    }
}
