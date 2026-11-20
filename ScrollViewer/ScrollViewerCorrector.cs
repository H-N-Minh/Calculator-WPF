//------------------------------------------------------------------------------
// © SDK GmbH
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp.Controls
{
    /// <summary>Attached property für ein ScrollViewer-Control, damit dieses nicht immer das MouseWheelEvent schluckt.</summary>
    /// <remarks>
    /// http://serialseb.blogspot.com/2007/09/wpf-tips-6-preventing-scrollviewer-from.html
    /// </remarks>
    public static class ScrollViewerCorrector
    {
        #region dependency properties

        public static readonly DependencyProperty FixScrollingProperty =
            DependencyProperty.RegisterAttached(
                "FixScrolling",
                typeof(bool),
                typeof(ScrollViewerCorrector),
                new FrameworkPropertyMetadata(false, OnFixScrollingPropertyChanged));

        public static readonly DependencyProperty DisableBringIntoViewProperty =
            DependencyProperty.RegisterAttached(
                "DisableBringIntoView",
                typeof(bool),
                typeof(ScrollViewerCorrector),
                new FrameworkPropertyMetadata(false, OnDisableBringIntoViewPropertyChanged));

        #endregion

        #region property getter/setter methods

        public static bool GetFixScrolling(DependencyObject d)
            => (bool)d.GetValue(FixScrollingProperty);

        public static void SetFixScrolling(DependencyObject d, bool value)
            => d.SetValue(FixScrollingProperty, value);

        public static bool GetDisableBringIntoView(DependencyObject d)
            => (bool)d.GetValue(DisableBringIntoViewProperty);

        public static void SetDisableBringIntoView(DependencyObject d, bool value)
            => d.SetValue(DisableBringIntoViewProperty, value);

        #endregion

        #region property changed callback

        private static void OnFixScrollingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ScrollViewer viewer))
            {
                throw new ArgumentException("FixScrolling can only be attached to a ScrollViewer.", nameof(d));
            }

            if ((bool)e.NewValue)
            {
                viewer.PreviewMouseWheel += Viewer_PreviewMouseWheel;
            }
            else
            {
                viewer.PreviewMouseWheel -= Viewer_PreviewMouseWheel;
            }
        }

        private static void OnDisableBringIntoViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ScrollViewer;
            if (viewer == null) { return; }

            if ((bool)e.NewValue)
            {
                viewer.RequestBringIntoView += Viewer_RequestBringIntoView;
            }
            else
            {
                viewer.RequestBringIntoView -= Viewer_RequestBringIntoView;
            }
        }

        #endregion

        #region event handlers

        private static void Viewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled || sender is not ScrollViewer scrollControl)
                return;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                var (hasInnerSv, canInnerScroll) = CheckInnerScrollViewers(e, scrollControl);
                if (canInnerScroll)
                    return;

                if (CanScrollVertically(scrollControl, e.Delta))
                {
                    ScrollTheCurrentViewer(hasInnerSv, e, scrollControl, sender);
                    return;
                }
            }

            ScrollTheParentViewer(sender, e, scrollControl);

            static void ScrollTheCurrentViewer(bool hasInnerSv, MouseWheelEventArgs e, ScrollViewer scrollControl, object sender)
            {
                if (!hasInnerSv)
                    return;

                e.Handled = true;
                scrollControl.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                });
            }

            static (bool hasInnerSv, bool canInnerScroll) CheckInnerScrollViewers(MouseWheelEventArgs e, ScrollViewer currentScrollViewer)
            {
                var hasInnerSv = false;
                var current = e.OriginalSource as DependencyObject;

                while (current is not null && current != currentScrollViewer)
                {
                    if (current is ScrollViewer innerSv)
                    {
                        hasInnerSv = true;
                        if (CanScrollVertically(innerSv, e.Delta))
                            return (true, true);
                    }

                    current = current is ContentElement ce
                        ? ContentOperations.GetParent(ce) ?? LogicalTreeHelper.GetParent(ce)
                        : VisualTreeHelper.GetParent(current);
                }

                return (hasInnerSv, false);
            }

            static bool CanScrollVertically(ScrollViewer sv, int delta)
            {
                if (sv.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled || sv.ScrollableHeight <= 0)
                    return false;

                return delta > 0
                    ? sv.VerticalOffset > 0.001
                    : sv.VerticalOffset < sv.ScrollableHeight - 0.001;
            }

            static void ScrollTheParentViewer(object sender, MouseWheelEventArgs e, ScrollViewer scrollControl)
            {
                e.Handled = true;
                if (VisualTreeHelper.GetParent(scrollControl) is UIElement parent)
                {
                    parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    });
                }
            }
        }


        private static void Viewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
            => e.Handled = true;

        #endregion
    }
}
