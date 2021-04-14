using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class FileSystemWatcherTests : IntegrationTestBase
    {
        private FileSystemWatcher _fsw;

        public List<FswEvent> Events { get; private set; }

        protected override void OnTestInitialize()
        {
            _fsw = new FileSystemWatcher(LogsPath, "*.csv");
            _fsw.NotifyFilter =
                //NotifyFilters.Attributes |
                //NotifyFilters.CreationTime |
                //NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                //NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                //NotifyFilters.Security |
                NotifyFilters.Size;
            _fsw.EnableRaisingEvents = true;

            Events = new List<FswEvent>();

            _fsw.Changed += (sender, e) => Events.Add(new FswEvent { ChangeType = e.ChangeType, Path = RelativePath.FromPathAndRoot(LogsPath, e.FullPath), });
            _fsw.Renamed += (sender, e) => Events.Add(new FswEvent { ChangeType = e.ChangeType, Path = RelativePath.FromPathAndRoot(LogsPath, e.FullPath), OldPath = RelativePath.FromPathAndRoot(LogsPath, e.OldFullPath) });
            _fsw.Deleted += (sender, e) => Events.Add(new FswEvent { ChangeType = e.ChangeType, OldPath = RelativePath.FromPathAndRoot(LogsPath, e.FullPath), });
        }

        [TestMethod]
        public async Task MyTestMethod()
        {
            LogHelper.Append(GetPath("test.csv"), LogHelper.Create("1"));
            LogHelper.Append(GetPath("test.csv"), LogHelper.Create("2"));
            LogHelper.Append(GetPath("test.csv"), LogHelper.Create("3"));
            await LogHelper.Rename(GetPath("test.csv"), GetPath("new.csv"));
            LogHelper.Append(GetPath("test.csv"), LogHelper.Create("4"));
            LogHelper.Append(GetPath("new.csv"), LogHelper.Create("5"));
            await Task.Delay(500);
        }
    }

    public class FswEvent
    {
        public RelativePath Path { get; set; }
        public RelativePath OldPath { get; set; }
        public WatcherChangeTypes ChangeType { get; set; }
    }
}
