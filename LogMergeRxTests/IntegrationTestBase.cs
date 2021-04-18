using System.IO;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public abstract class IntegrationTestBase
    {
        public TestContext TestContext { get; set; }

        protected AbsolutePath LogsPath { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            LogsPath = (AbsolutePath)Path.Combine(TestContext.TestRunDirectory, "logs", TestContext.TestName);
            Directory.CreateDirectory(LogsPath);

            OnTestInitialize();
        }

        protected virtual void OnTestInitialize()
        {
        }

        protected AbsolutePath GetPath(string fileName) =>
            (AbsolutePath)Path.Combine(LogsPath, fileName);

    }
}
