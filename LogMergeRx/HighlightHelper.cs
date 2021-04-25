using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LogMergeRx
{
    public static class HighlightHelper
    {
        private static readonly Brush highlightBrush = new SolidColorBrush(Colors.MediumAquamarine);

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(HighlightHelper), new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.RegisterAttached("Highlight", typeof(string), typeof(HighlightHelper), new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

        public static string GetText(DependencyObject d) =>
            (string)d.GetValue(TextProperty);

        public static void SetText(DependencyObject d, string value) =>
            d.SetValue(TextProperty, value);

        public static string GetHighlight(DependencyObject d) =>
            (string)d.GetValue(HighlightProperty);

        public static void SetHighlight(DependencyObject d, string value) =>
            d.SetValue(HighlightProperty, value);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            SetHighlightedText((TextBlock)d, GetText(d), GetHighlight(d));

        private static void SetHighlightedText(TextBlock textBlock, string text, string highlight)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                textBlock.Text = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(highlight) ||
                highlight.Length < 3 ||
                !text.Contains(highlight, StringComparison.OrdinalIgnoreCase))
            {
                textBlock.Text = text;
                return;
            }

            textBlock.Text = string.Empty; // reset
            textBlock.Inlines.AddRange(
                Split().Select(GetRun));

            Run GetRun(string textPart) =>
                string.Equals(textPart, highlight, StringComparison.OrdinalIgnoreCase)
                    ? new Run { Text = textPart, FontWeight = FontWeights.ExtraBold, Foreground = highlightBrush }
                    : new Run { Text = textPart };

            IEnumerable<string> Split() =>
                RegexCache
                    .GetRegex($@"({Regex.Escape(highlight)})")
                    .Split(text)
                    .Where(p => p != string.Empty);
        }
    }
}
