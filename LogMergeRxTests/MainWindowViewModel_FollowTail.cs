using System;
using System.Threading.Tasks;
using FluentAssertions;
using LogMergeRx;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class MainWindowViewModel_FollowTail_Tests
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModel_FollowTail_Tests()
        {
            _viewModel = new MainWindowViewModel(TimeSpan.Zero);
            _viewModel.ItemsSource.Add(LogHelper.Create("message error 1", LogLevel.ERROR));
            _viewModel.ItemsSource.Add(LogHelper.Create("message error 2", LogLevel.ERROR));
            _viewModel.ItemsSource.Add(LogHelper.Create("message warning 1", LogLevel.WARN));
            _viewModel.ItemsSource.Add(LogHelper.Create("message warning 1", LogLevel.WARN));
            _viewModel.ItemsSource.Add(LogHelper.Create("message notice 1", LogLevel.INFO));
            _viewModel.ItemsSource.Add(LogHelper.Create("message info 1", LogLevel.INFO));
        }

        [TestMethod]
        public void Setting_SearchRegex_Disables_Follow_Tail()
        {
            _viewModel.FollowTail.Value = true;

            _viewModel.SearchRegex.Value = "xxx";

            DispatcherUtil.DoEvents(); // We observe on dispatcher
            DispatcherUtil.DoEvents(); // We observe on dispatcher

            _viewModel.FollowTail.Value.Should().BeFalse();
        }

        [TestMethod]
        public void FollowTail_True_Adding_New_Item_Scrolls_To_End()
        {
            _viewModel.FollowTail.Value = true;

            var newEntry = LogHelper.Create("new message 1", LogLevel.ERROR);
            _viewModel.ItemsSource.Add(newEntry);

            DispatcherUtil.DoEvents(); // We observe on dispatcher

            _viewModel.ScrollToItem.Value.Should().Be(newEntry);
            _viewModel.ScrollToIndex.Value.Should().Be(_viewModel.ItemsSource.Count - 1);

            newEntry = LogHelper.Create("new message 2", LogLevel.ERROR);
            _viewModel.ItemsSource.Add(newEntry);

            DispatcherUtil.DoEvents(); // We observe on dispatcher

            _viewModel.ScrollToItem.Value.Should().Be(newEntry);
            _viewModel.ScrollToIndex.Value.Should().Be(_viewModel.ItemsSource.Count - 1);
        }
    }
}
