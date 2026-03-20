using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.Reactive.Testing;
using Xunit;
using Xunit.Abstractions;

namespace LogMergeRx;

public class MainWindowViewModel_Filter_Tests
{
    private readonly MainWindowViewModel _viewModel;
    private readonly TestScheduler _scheduler;

    public MainWindowViewModel_Filter_Tests(ITestOutputHelper output)
    {
        _output = output;
        _scheduler = new TestScheduler();
        _viewModel = new MainWindowViewModel(_scheduler);

        // Use reflection to get the test name from ITestOutputHelper
        var testField = output.GetType()
            .GetField("test", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var test = testField?.GetValue(output);
        var testName = test?.GetType().GetProperty("TestCase")?.GetValue(test)
            ?.GetType().GetProperty("TestMethod")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test))
            ?.GetType().GetProperty("Method")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test)?.GetType().GetProperty("TestMethod")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test)))
            ?.GetType().GetProperty("Name")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test)?.GetType().GetProperty("TestMethod")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test))?.GetType().GetProperty("Method")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test)?.GetType().GetProperty("TestMethod")?.GetValue(test?.GetType().GetProperty("TestCase")?.GetValue(test))));

        _output.WriteLine($"=== CONSTRUCTOR for: {testName ?? "Unknown"} ===");

        _output.WriteLine($"view:{View.Count()}; filters:{string.Join(",", _viewModel.Filters.Where(x => x.IsFiltered()).SelectMany(x => x.GetFilterValues()))};");

        _viewModel.AddItems(ImmutableArray.Create(
            LogHelper.Create("message error 1", LogLevel.ERROR, fileId: 0),
            LogHelper.Create("message error 2", LogLevel.ERROR, fileId: 1),
            LogHelper.Create("message warning 1", LogLevel.WARN, fileId: 2),
            LogHelper.Create("message warning 2", LogLevel.WARN, fileId: 0),
            LogHelper.Create("message notice 1", LogLevel.NOTICE, fileId: 1),
            LogHelper.Create("message info 1", LogLevel.INFO, fileId: 2)));

        _scheduler.AdvanceBy(DefaultThrottle.Ticks);
        _scheduler.AdvanceBy(DefaultThrottle.Ticks);

        _output.WriteLine($"view:{View.Count()}; filters:{string.Join(",", _viewModel.Filters.Where(x => x.IsFiltered()).SelectMany(x => x.GetFilterValues()))};");
    }

    private IEnumerable<LogEntry> View =>
        _viewModel.ItemsSourceView.OfType<LogEntry>();

    private static readonly TimeSpan DefaultThrottle = TimeSpan.FromMilliseconds(510);
    private readonly ITestOutputHelper _output;

    private void DoAndWait(Action action)
    {
        action();
        _scheduler.AdvanceBy(DefaultThrottle.Ticks);
    }

    [Fact]
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

    [Fact]
    public void Include_warnings()
    {
        _viewModel.LevelFilterViewModel.ShowErrors.Value = false;
        _viewModel.LevelFilterViewModel.ShowInfos.Value = false;
        _viewModel.LevelFilterViewModel.ShowNotices.Value = false;

        View.Should().HaveCount(2);
        View.Select(x => x.Message).Should().Equal("message warning 1", "message warning 2");
    }

    [Fact]
    public void Include_notice()
    {
        _viewModel.LevelFilterViewModel.ShowErrors.Value = false;
        _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
        _viewModel.LevelFilterViewModel.ShowInfos.Value = false;

        View.Should().HaveCount(1);
        View.Select(x => x.Message).Should().Equal("message notice 1");
    }

    [Fact]
    public void Include_info()
    {
        _viewModel.LevelFilterViewModel.ShowErrors.Value = false;
        _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
        _viewModel.LevelFilterViewModel.ShowNotices.Value = false;

        _viewModel.LevelFilterViewModel.ShowInfos.Value = true;

        View.Should().HaveCount(1);
        View.Select(x => x.Message).Should().Equal("message info 1");
    }

    [Fact]
    public void Include_all()
    {
        View.Should().HaveCount(6);
        View.Select(x => x.Message).Should().Equal("message error 1", "message error 2", "message warning 1", "message warning 2", "message notice 1", "message info 1");
    }

    [Fact]
    public void Remove_all()
    {
        _viewModel.LevelFilterViewModel.ShowErrors.Value = false; // default
        _viewModel.LevelFilterViewModel.ShowWarnings.Value = false;
        _viewModel.LevelFilterViewModel.ShowNotices.Value = false;
        _viewModel.LevelFilterViewModel.ShowInfos.Value = false;

        View.Should().BeEmpty();
    }

    [Fact]
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

    [Fact]
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
