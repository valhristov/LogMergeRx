using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class MainWindowViewModel_Date_Ranges
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModel_Date_Ranges()
        {
            _viewModel = new MainWindowViewModel(TimeSpan.Zero);
        }

        [TestMethod]
        public void Adding_Items_For_The_First_Time()
        {
            // Arrange

            var first = LogHelper.Create("first");
            var second = LogHelper.Create("second");

            // Act
            _viewModel.AddItems(ImmutableList.Create(first, second));

            // Assert
            _viewModel.FirstItemSeconds.Value.Should().Be(DateTimeHelper.FromDateToSeconds(first.Date));
            _viewModel.VisibleRangeStart.Value.Should().Be(DateTimeHelper.FromDateToSeconds(first.Date));

            _viewModel.LastItemSeconds.Value.Should().Be(DateTimeHelper.FromDateToSeconds(second.Date));
            _viewModel.VisibleRangeEnd.Value.Should().Be(DateTimeHelper.FromDateToSeconds(second.Date));
        }

        [TestMethod]
        public void Adding_Items_For_Second_Time_1()
        {
            // Arrange
            var first = GetLogEntry(TimeSpan.FromSeconds(100));
            var second = GetLogEntry(TimeSpan.FromSeconds(101));
            _viewModel.AddItems(ImmutableList.Create(first, second));

            // We didn't touch VisibleRangeStart/End and they should change

            // Act
            var earlier = GetLogEntry(TimeSpan.FromSeconds(50));
            var later = GetLogEntry(TimeSpan.FromSeconds(150));
            _viewModel.AddItems(ImmutableList.Create(earlier, later));

            // Assert
            _viewModel.FirstItemSeconds.Value.Should().Be(DateTimeHelper.FromDateToSeconds(earlier.Date));
            _viewModel.VisibleRangeStart.Value.Should().Be(DateTimeHelper.FromDateToSeconds(earlier.Date));

            _viewModel.LastItemSeconds.Value.Should().Be(DateTimeHelper.FromDateToSeconds(later.Date));
            _viewModel.VisibleRangeEnd.Value.Should().Be(DateTimeHelper.FromDateToSeconds(later.Date));
        }

        [TestMethod]
        public void Adding_Items_For_Second_Time_2()
        {
            // Arrange
            var first = GetLogEntry(TimeSpan.FromSeconds(100));
            var second = GetLogEntry(TimeSpan.FromSeconds(110));
            _viewModel.AddItems(ImmutableList.Create(first, second));

            // Change VisibleRangeStart/End
            _viewModel.VisibleRangeStart.Value = DateTimeHelper.FromDateToSeconds(GetDate(TimeSpan.FromSeconds(101)));
            _viewModel.VisibleRangeEnd.Value = DateTimeHelper.FromDateToSeconds(GetDate(TimeSpan.FromSeconds(120)));

            // Act
            var earlier = GetLogEntry(TimeSpan.FromSeconds(50));
            var later = GetLogEntry(TimeSpan.FromSeconds(150));
            _viewModel.AddItems(ImmutableList.Create(earlier, later));

            // Assert
            _viewModel.FirstItemSeconds.Value.Should().Be(DateTimeHelper.FromDateToSeconds(earlier.Date));
            _viewModel.VisibleRangeStart.Value.Should().Be(DateTimeHelper.FromDateToSeconds(first.Date));

            _viewModel.LastItemSeconds.Value.Should().Be(DateTimeHelper.FromDateToSeconds(later.Date));
            _viewModel.VisibleRangeEnd.Value.Should().Be(DateTimeHelper.FromDateToSeconds(second.Date));
        }

        private static DateTime GetDate(TimeSpan offset) =>
            new DateTime(2021, 3, 25).Add(offset);

        private static LogEntry GetLogEntry(TimeSpan offset) =>
            new LogEntry(new FileId(0), GetDate(offset), LogLevel.ERROR, "", "some");
    }
}
