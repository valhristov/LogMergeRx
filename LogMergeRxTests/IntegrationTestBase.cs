using System;
using System.IO;
using LogMergeRx.Model;

namespace LogMergeRx;

public abstract class IntegrationTestBase : IDisposable
{
    protected AbsolutePath LogsPath { get; private set; }

    protected IntegrationTestBase()
    {
        var testName = GetType().Name;
        var tempPath = Path.Combine(Path.GetTempPath(), "LogMergeRxTests", testName, Guid.NewGuid().ToString());
        LogsPath = (AbsolutePath)Path.Combine(tempPath, "logs");
        Directory.CreateDirectory(LogsPath);

        OnTestInitialize();
    }

    protected virtual void OnTestInitialize()
    {
    }

    protected AbsolutePath GetPath(string fileName) =>
        (AbsolutePath)Path.Combine(LogsPath, fileName);

    public virtual void Dispose()
    {
        // Cleanup if needed
    }
}
