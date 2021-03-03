using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class MainWindowViewModel_Filter_Tests
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModel_Filter_Tests()
        {
            _viewModel = new MainWindowViewModel();
            _viewModel.ItemsSource.AddRange(
                new[]
                {
                    CreateLogEntry("ERROR", "message error 1"),
                    CreateLogEntry("error", "message error 2"),
                    CreateLogEntry("WARN", "message warning 1"),
                    CreateLogEntry("warn", "message warning 2"),
                    CreateLogEntry("NOTice", "message notice 1"),
                    CreateLogEntry("INFO", "message info 1"),
                });

            LogEntry CreateLogEntry(string level, string message) =>
                new LogEntry("", "", level, "source", message);
        }

        private IEnumerable<LogEntry> View =>
            CollectionViewSource.GetDefaultView(_viewModel.ItemsSource).OfType<LogEntry>();

        [TestMethod]
        public void Include_errors()
        {
            _viewModel.ShowWarnings.Value = false;
            _viewModel.ShowInfos.Value = false;
            _viewModel.ShowNotices.Value = false;

            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message error 1", "message error 2");

            _viewModel.ShowErrors.Value = false;
            View.Should().BeEmpty();
        }

        [TestMethod]
        public void Include_warnings()
        {
            _viewModel.ShowErrors.Value = false;
            _viewModel.ShowInfos.Value = false;
            _viewModel.ShowNotices.Value = false;

            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message warning 1", "message warning 2");
        }

        [TestMethod]
        public void Include_notice()
        {
            _viewModel.ShowErrors.Value = false;
            _viewModel.ShowWarnings.Value = false;
            _viewModel.ShowInfos.Value = false;

            View.Should().HaveCount(1);
            View.Select(x => x.Message).Should().Equal("message notice 1");
        }

        [TestMethod]
        public void Include_info()
        {
            _viewModel.ShowErrors.Value = false;
            _viewModel.ShowWarnings.Value = false;
            _viewModel.ShowNotices.Value = false;

            _viewModel.ShowInfos.Value = true;
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
            _viewModel.ShowErrors.Value = false; // default
            _viewModel.ShowWarnings.Value = false;
            _viewModel.ShowNotices.Value = false;
            _viewModel.ShowInfos.Value = false;
            View.Should().BeEmpty();
        }

        [TestMethod]
        public void Include_regex()
        {
            _viewModel.IncludeRegex.Value = string.Empty; // default
            View.Should().HaveCount(6); // all items

            _viewModel.IncludeRegex.Value = "2";
            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message error 2", "message warning 2");

            _viewModel.IncludeRegex.Value = "message.*\\s1";
            View.Should().HaveCount(4);
            View.Select(x => x.Message).Should().Equal("message error 1", "message warning 1", "message notice 1", "message info 1");
        }

        [TestMethod]
        public void Exclude_regex()
        {
            _viewModel.ExcludeRegex.Value = string.Empty; // default
            View.Should().HaveCount(6); // all items

            _viewModel.ExcludeRegex.Value = "2";
            View.Should().HaveCount(4);
            View.Select(x => x.Message).Should().Equal("message error 1", "message warning 1", "message notice 1", "message info 1");

            _viewModel.ExcludeRegex.Value = "message.*\\s1";
            View.Should().HaveCount(2);
            View.Select(x => x.Message).Should().Equal("message error 2", "message warning 2");
        }
    }
}
