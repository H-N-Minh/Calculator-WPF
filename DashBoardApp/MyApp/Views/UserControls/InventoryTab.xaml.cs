using DashBoardApp.Models;
using System;
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

namespace DashBoardApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for InventoryTab.xaml
    /// </summary>
    public partial class InventoryTab : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(ObservableCollection<HardwareAsset>),
                typeof(InventoryTab),
                new PropertyMetadata(null));

        public ObservableCollection<HardwareAsset> ItemsSource
        {
            get => (ObservableCollection<HardwareAsset>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public InventoryTab()
        {
            InitializeComponent();
        }
    }
}
