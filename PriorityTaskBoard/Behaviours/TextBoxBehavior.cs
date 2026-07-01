using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PriorityTaskBoard.Behaviours;

public static class TextBoxBehavior
{
    public static readonly DependencyProperty SelectAllOnFocus =
                    DependencyProperty.RegisterAttached(
                        "SelectAllOnFocus",
                        typeof(bool),
                        typeof(TextBoxBehavior),
                        new PropertyMetadata(false, OnPropertyChangedCallBack));

    public static void SetSelectAllOnFocus(DependencyObject obj, bool value) => obj.SetValue(SelectAllOnFocus, value);
    public static bool GetSelectAllOnFocus(DependencyObject obj) => (bool)obj.GetValue(SelectAllOnFocus);

    public static void OnPropertyChangedCallBack(DependencyObject obj,  DependencyPropertyChangedEventArgs e)
    {
        if (obj is TextBox tb)
        {
            if ((bool)e.NewValue)
            {
                tb.PreviewMouseDown += SelectAllText;
            }
            else tb.PreviewMouseDown -= SelectAllText;
        }
    }

    public static void SelectAllText(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.Focus();
            tb.SelectAll();
            e.Handled = true;
        }
    }

}
