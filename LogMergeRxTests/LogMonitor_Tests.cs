using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LogMergeRx
{
    [TestClass]
    public class LogMonitor_Tests : IntegrationTestBase
    {
        private List<FileId> Files { get; set; }
        private List<LogEntry> Entries { get; set; }
        private LogMonitor LogMonitor { get; set; }

        protected override void OnTestInitialize()
        {
            Files = new List<FileId>();
            Entries = new List<LogEntry>();

            LogMonitor = new LogMonitor(LogsPath);
            LogMonitor.ChangedFiles.Subscribe(x => Files.Add(x.Id));
            LogMonitor.ReadEntries.Subscribe(Entries.AddRange);
        }

        [TestMethod]
        public async Task Read_Modified_Files()
        {
            LogHelper.AppendHeaders(GetPath("log1.csv"));
            LogHelper.AppendHeaders(GetPath("log2.csv"));

            LogMonitor.Start();

            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("1"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("2"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("3"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("4"));

            await Task.Delay(100);

            Files.Select(x => x.Id).Distinct().Should().Equal(1, 2);
            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }

        [TestMethod]
        public async Task Read_Existing_Files()
        {
            LogHelper.AppendHeaders(GetPath("log1.csv"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("1"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("2"));
            LogHelper.AppendHeaders(GetPath("log2.csv"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("3"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("4"));

            LogMonitor.Start();

            await Task.Delay(500);

            Files.Select(x => x.Id).Distinct().Should().Equal(1, 2);

            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }

        [TestMethod]
        public async Task Read_Overflown_Files()
        {
            LogMonitor.Start();

            LogHelper.AppendHeaders(GetPath("log1.csv"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("1"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("2"));

            await Task.Delay(500);

            await LogHelper.Rename(GetPath("log1.csv"), GetPath("log2.csv"));

            LogHelper.AppendHeaders(GetPath("log1.csv"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("3"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("4"));

            await Task.Delay(500);

            Files.Select(x => x.Id).Distinct().Should().Equal(1, 2);

            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }
    }
}
