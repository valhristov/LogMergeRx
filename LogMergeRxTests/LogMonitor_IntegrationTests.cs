using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class LogMonitor_IntegrationTests : IntegrationTestBase
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
            LogMonitor.ReadEntries.Subscribe(x => Entries.AddRange(x));

            LogMonitor.Start();
        }

        [TestMethod]
        public async Task ParallelMonitoring()
        {
            Task.WaitAll(
                Task.Run(async () => await WriteLogsAsync("a.csv", 'A')),
                Task.Run(async () => await WriteLogsAsync("b.csv", 'B')),
                Task.Run(async () => await WriteLogsAsync("c.csv", 'C')),
                Task.Run(async () => await WriteLogsAsync("d.csv", 'D'))
                );

            await Task.Delay(3000);

            AssertEntries('A');
            AssertEntries('B');
            AssertEntries('C');
            AssertEntries('D');

            // Expect 1100 entries that start with the provided prefix
            void AssertEntries(char prefix)
            {
                var entries = Entries.Where(e => e.Message.StartsWith(prefix));
                var expected = Enumerable.Range(0, 1100).Select(i => $"{prefix}{i:0000}").ToHashSet();

                var actual = entries.Select(e => e.Message).ToHashSet();
                expected.Except(actual).Should().BeEmpty();
            }

            async Task<int> WriteLogsAsync(string fileName, char messagePrefix)
            {
                int files = 0;
                int messages = 0;

                await WriteFile();
                while (files < 10)
                {
                    await LogHelper.Rename(GetPath(fileName), GetPath($"{Path.GetFileNameWithoutExtension(fileName)}{files++}{Path.GetExtension(fileName)}"));
                    await WriteFile();
                }

                return messages;

                async Task WriteFile()
                {
                    LogHelper.AppendHeaders(GetPath(fileName));
                    for (int i = 0; i < 100; i++)
                    {
                        LogHelper.Append(GetPath(fileName), LogHelper.Create($"{messagePrefix}{messages++:0000}"));
                        await Task.Delay(1);
                    }
                }
            }
        }

    }
}
