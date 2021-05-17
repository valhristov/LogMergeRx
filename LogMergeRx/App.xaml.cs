using System;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using LogMergeRx.Demo;
using LogMergeRx.Model;
using Neat.Results;

namespace LogMergeRx
{
    public partial class App : Application
    {
        // Need to keep a reference to prevent the GC from collecting the monitor
        private LogMonitor _monitor;

        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeCurrentLanguageForWPF();

            GetLogsPath()
                .Match(path =>
                {
                    if (path.Value.Contains("log-demo"))
                    {
                        LogGenerator.Start(path);
                    }

                    var viewModel = new MainWindowViewModel(DispatcherScheduler.Current);

                    _monitor = new LogMonitor(path);

                    _monitor.ChangedFiles
                        .ObserveOnDispatcher()
                        // Add changed files to the filter
                        .Subscribe(viewModel.AddFileToFilter);

                    _monitor.RenamedFiles
                        .ObserveOnDispatcher()
                        // Update renamed file names. File ID remains the same
                        .Subscribe(viewModel.UpdateFileName);

                    _monitor.ReadEntries
                        .ObserveOnDispatcher()
                        // Add new entries
                        .Subscribe(viewModel.AddItems);

                    _monitor.Start();

                    MainWindow = new MainWindow(viewModel);
                    MainWindow.Title = $"LogMerge: {path}";
                    MainWindow.Show();

                    // For some reason after the FolderBrowserDialog closes, the
                    // main window is not activated when shown.
                    MainWindow.Activate();
                },
                errors =>
                {
                    Shutdown();
                });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LogGenerator.Stop();
            base.OnExit(e);
        }

        private static Result<AbsolutePath> GetLogsPath()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? Result.Success((AbsolutePath)dialog.SelectedPath)
                : Result.Failure<AbsolutePath>("User did not choose a directory.");
        }

        /// <summary>
        /// Source: https://github.com/dotnet/wpf/issues/1946#issuecomment-534564980
        /// </summary>
        private static void InitializeCurrentLanguageForWPF()
        {
            // Create a made-up IETF language tag "more specific" than the culture we are based on.
            // This allows all standard logic regarding IETF language tag hierarchy to still make sense and we are
            // compatible with the fact that we may have overridden language specific defaults with Windows OS settings.
            var culture = CultureInfo.CurrentCulture;
            var language = XmlLanguage.GetLanguage(culture.IetfLanguageTag + "-current");
            var type = typeof(XmlLanguage);
            const BindingFlags kField = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            type.GetField("_equivalentCulture", kField).SetValue(language, culture);
            type.GetField("_compatibleCulture", kField).SetValue(language, culture);
            if (culture.IsNeutralCulture)
                culture = CultureInfo.CreateSpecificCulture(culture.Name);
            type.GetField("_specificCulture", kField).SetValue(language, culture);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(language));
        }
    }
}
