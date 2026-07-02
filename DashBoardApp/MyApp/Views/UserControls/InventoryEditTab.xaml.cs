using DashBoardApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DashBoardApp.Views.UserControls;

/// <summary>
/// Interaction logic for InventoryEditTab.xaml
/// </summary>
public partial class InventoryEditTab : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(ObservableCollection<HardwareAsset>),
            typeof(InventoryEditTab),
            new PropertyMetadata(null));

    public ObservableCollection<HardwareAsset> ItemsSource
    {
        get => (ObservableCollection<HardwareAsset>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty EmployeesSourceProperty =
        DependencyProperty.Register(
            nameof(EmployeesSource),
            typeof(IEnumerable),
            typeof(InventoryEditTab),
            new PropertyMetadata(null));

    public IEnumerable EmployeesSource
    {
        get => (IEnumerable)GetValue(EmployeesSourceProperty);
        set => SetValue(EmployeesSourceProperty, value);
    }

    public InventoryEditTab()
    {
        InitializeComponent();
    }
}
