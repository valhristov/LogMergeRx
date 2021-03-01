using System;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class LogViewerViewModel_Search_tests
    {
        private readonly LogViewerViewModel _viewModel;

        public LogViewerViewModel_Search_tests()
        {
            _viewModel = new LogViewerViewModel();
            Array.ForEach(
                new[]
                {
                    // only errors are enabled by default
                    CreateLogEntry("ERROR", "1"), // index:0
                    CreateLogEntry("ERROR", "1"), // index:1
                    CreateLogEntry("ERROR", "1"), // index:2
                    CreateLogEntry("WARN", "2"), //  index:3  // index:0 with removed errors
                    CreateLogEntry("ERROR", "3"), // index:4
                    CreateLogEntry("WARN", "2"), //  index:5  // index:1 with removed errors
                    CreateLogEntry("WARN", "2"), //  index:6  // index:2 with removed errors
                    CreateLogEntry("WARN", "3"), //  index:7  // index:3 with removed errors
                    CreateLogEntry("WARN", "2"), //  index:8  // index:4 with removed errors
                    CreateLogEntry("WARN", "2"), //  index:9  // index:5 with removed errors
                    CreateLogEntry("WARN", "1"), //  index:10 // index:6 with removed errors
                },
                _viewModel.ItemsSource.Add);

            LogEntry CreateLogEntry(string level, string message) =>
                new LogEntry("", "", level, "source", message);
        }

        [TestMethod]
        public void SearchRegex_always_searches_from_start()
        {
            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            _viewModel.SearchRegex.Value = "1";
            _viewModel.ScrollToIndex.Value.Should().Be(0);

            _viewModel.SearchRegex.Value = "2";
            _viewModel.ScrollToIndex.Value.Should().Be(3);

            _viewModel.SearchRegex.Value = "3";
            _viewModel.ScrollToIndex.Value.Should().Be(4);

            _viewModel.SearchRegex.Value = "1";
            _viewModel.ScrollToIndex.Value.Should().Be(0);
        }

        [TestMethod]
        public void NextIndex_goes_forward()
        {
            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            _viewModel.SearchRegex.Value = "2";
            _viewModel.ScrollToIndex.Value.Should().Be(3);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(5);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(6);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(8);

            _viewModel.NextIndex.Execute(null);
            _viewModel.ScrollToIndex.Value.Should().Be(9);

            _viewModel.NextIndex.Execute(null); // cannot find more, stay on the last
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
            _viewModel.ShowErrors.Value = false; // remove errors, the indexes will shift
            _viewModel.ScrollToIndex.Value.Should().Be(0); // default

            _viewModel.SearchRegex.Value = "2";
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
