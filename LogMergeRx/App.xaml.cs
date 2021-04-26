using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace LogMergeRx
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeCurrentLanguageForWPF();
        }

        /// <summary>
        /// Source: https://github.com/dotnet/wpf/issues/1946#issuecomment-534564980
        /// </summary>
        public static void InitializeCurrentLanguageForWPF()
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
