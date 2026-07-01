using PriorityTaskBoard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PriorityTaskBoard.Controls;

public partial class SingleEntry : UserControl
{
    public SingleEntry()
    {
        InitializeComponent();
    }

    // DP for prioritylevel
    public TaskPriority PriorityLevel
    {
        get => (TaskPriority)GetValue(PriorityLevelProperty);
        set => SetValue(PriorityLevelProperty, value);
    }

    public static readonly DependencyProperty PriorityLevelProperty =
        DependencyProperty.Register(
            nameof(PriorityLevel),
            typeof(TaskPriority),
            typeof(SingleEntry),
            new PropertyMetadata(TaskPriority.Low, OnPriorityLevelChanged));

    public static void OnPriorityLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SingleEntry pb && e.NewValue is TaskPriority priority)
        {
            pb.UpdateBadge(priority);
        }
    }

    private void UpdateBadge(TaskPriority priorityLevel)
    {
        BadgeText.Text = priorityLevel.ToString();
        BadgeBorder.Background = priorityLevel switch
        {
            TaskPriority.High => new SolidColorBrush(Colors.Crimson),
            TaskPriority.Medium => new SolidColorBrush(Colors.DarkOrange),
            TaskPriority.Low => new SolidColorBrush(Colors.ForestGreen),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }


    // DP for IsChecked
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckProperty);
        set => SetValue(IsCheckProperty, value);
    }

    public static readonly DependencyProperty IsCheckProperty =
        DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(SingleEntry),
            new PropertyMetadata(false, OnIsCheckChanged));

    public static void OnIsCheckChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SingleEntry pb && e.NewValue is bool b)
        {
            pb.MyCheckBox.IsChecked = b;
        }
    }

    // DP for Text
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SingleEntry),
            new PropertyMetadata("", OnTextChanged));

    public static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SingleEntry pb && e.NewValue is string str)
        {
            pb.MyTextBlock.Text = str;
        }
    }
}
