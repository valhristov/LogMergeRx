using FluentAssertions;
using LogMergeRx;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRxTests
{
    [TestClass]
    public class MainWindowViewModel_FollowTail_Tests
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModel_FollowTail_Tests()
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
        }

        private static int counter = 0;
        private static LogEntry CreateLogEntry(string level, string message) =>
            new LogEntry("file", counter++.ToString("00"), level, "source", message);

        [TestMethod]
        public void Setting_SearchRegex_Disables_Follow_Tail()
        {
            _viewModel.FollowTail.Value = true;

            _viewModel.SearchRegex.Value = "xxx";

            _viewModel.FollowTail.Value.Should().BeFalse();
        }

        [TestMethod]
        public void FollowTail_True_Adding_New_Item_Scrolls_To_End()
        {
            _viewModel.FollowTail.Value = true;

            var newEntry = CreateLogEntry("ERROR", "new message 1");
            _viewModel.ItemsSource.Add(newEntry);

            _viewModel.ScrollToItem.Value.Should().Be(newEntry);
            _viewModel.ScrollToIndex.Value.Should().Be(_viewModel.ItemsSource.Count - 1);

            newEntry = CreateLogEntry("ERROR", "new message 2");
            _viewModel.ItemsSource.Add(newEntry);

            _viewModel.ScrollToItem.Value.Should().Be(newEntry);
            _viewModel.ScrollToIndex.Value.Should().Be(_viewModel.ItemsSource.Count - 1);
        }
    }
}
