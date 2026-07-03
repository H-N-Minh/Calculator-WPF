using System;
using System.Collections.Generic;
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

namespace DashBoardApp.Controls
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class CustomStatusBar : UserControl
    {
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                                        nameof(IsLoading),
                                        typeof(bool),
                                        typeof(CustomStatusBar),
                                        new PropertyMetadata(false)
                                        );
        public CustomStatusBar()
        {
            InitializeComponent();
        }
    }
}
