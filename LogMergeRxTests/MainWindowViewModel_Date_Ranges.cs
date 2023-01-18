using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
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
            _viewModel = new MainWindowViewModel(Scheduler.Default);
        }

        [TestMethod]
        public void Adding_Items_For_The_First_Time()
        {
            // Arrange

            var first = LogHelper.Create("first");
            var second = LogHelper.Create("second");

            // Act
            _viewModel.AddItems(ImmutableArray.Create(first, second));

            // Assert
            _viewModel.DateFilterViewModel.Minimum.Value.Should().Be(DateTimeHelper.FromDateToSeconds(first.Date));
            _viewModel.DateFilterViewModel.Start.Value.Should().Be(DateTimeHelper.FromDateToSeconds(first.Date));

            _viewModel.DateFilterViewModel.Maximum.Value.Should().Be(DateTimeHelper.FromDateToSeconds(second.Date));
            _viewModel.DateFilterViewModel.End.Value.Should().Be(DateTimeHelper.FromDateToSeconds(second.Date));
        }

        [TestMethod]
        public void Adding_Items_For_Second_Time_1()
        {
            // Arrange
            var first = GetLogEntry(TimeSpan.FromSeconds(100));
            var second = GetLogEntry(TimeSpan.FromSeconds(101));
            _viewModel.AddItems(ImmutableArray.Create(first, second));

            // We didn't touch VisibleRangeStart/End and they should change

            // Act
            var earlier = GetLogEntry(TimeSpan.FromSeconds(50));
            var later = GetLogEntry(TimeSpan.FromSeconds(150));
            _viewModel.AddItems(ImmutableArray.Create(earlier, later));

            // Assert
            _viewModel.DateFilterViewModel.Minimum.Value.Should().Be(DateTimeHelper.FromDateToSeconds(earlier.Date));
            _viewModel.DateFilterViewModel.Start.Value.Should().Be(DateTimeHelper.FromDateToSeconds(earlier.Date));

            _viewModel.DateFilterViewModel.Maximum.Value.Should().Be(DateTimeHelper.FromDateToSeconds(later.Date));
            _viewModel.DateFilterViewModel.End.Value.Should().Be(DateTimeHelper.FromDateToSeconds(later.Date));
        }

        [TestMethod]
        public void Adding_Items_For_Second_Time_2()
        {
            // Arrange
            var first = GetLogEntry(TimeSpan.FromSeconds(100));
            var second = GetLogEntry(TimeSpan.FromSeconds(110));
            _viewModel.AddItems(ImmutableArray.Create(first, second));

            // Change VisibleRangeStart/End
            var newVisibleRangeStart = DateTimeHelper.FromDateToSeconds(GetDate(TimeSpan.FromSeconds(101)));
            _viewModel.DateFilterViewModel.Start.Value = newVisibleRangeStart;
            var newVisibleRangeEnd = DateTimeHelper.FromDateToSeconds(GetDate(TimeSpan.FromSeconds(120)));
            _viewModel.DateFilterViewModel.End.Value = newVisibleRangeEnd;

            // Act
            var earlier = GetLogEntry(TimeSpan.FromSeconds(50));
            var later = GetLogEntry(TimeSpan.FromSeconds(150));
            _viewModel.AddItems(ImmutableArray.Create(earlier, later));

            // Assert
            _viewModel.DateFilterViewModel.Minimum.Value.Should().Be(DateTimeHelper.FromDateToSeconds(earlier.Date));
            _viewModel.DateFilterViewModel.Start.Value.Should().Be(newVisibleRangeStart);

            _viewModel.DateFilterViewModel.Maximum.Value.Should().Be(DateTimeHelper.FromDateToSeconds(later.Date));
            _viewModel.DateFilterViewModel.End.Value.Should().Be(newVisibleRangeEnd);
        }

        private static DateTime GetDate(TimeSpan offset) =>
            new DateTime(2021, 3, 25).Add(offset);

        private static LogEntry GetLogEntry(TimeSpan offset) =>
            new LogEntry(new FileId(0), GetDate(offset), LogLevel.ERROR, "", "some");
    }
}
