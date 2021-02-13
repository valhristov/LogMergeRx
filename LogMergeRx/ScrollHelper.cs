using System.Windows;
using System.Windows.Controls;

namespace LogMergeRx
{
    public static class ScrollHelper
    {
        public static readonly DependencyProperty ScrollToIndexProperty =
            DependencyProperty.RegisterAttached("ScrollToIndex", typeof(int), typeof(ScrollHelper), new PropertyMetadata(0, OnScrollToIndexChanged));

        public static int GetScrollToIndex(DependencyObject d) =>
            (int)d.GetValue(ScrollToIndexProperty);

        public static void SetScrollToIndex(DependencyObject d, int value) =>
            d.SetValue(ScrollToIndexProperty, value);

        private static void OnScrollToIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            (d as VirtualizingStackPanel)?.BringIndexIntoViewPublic((int)e.NewValue);
    }
}
