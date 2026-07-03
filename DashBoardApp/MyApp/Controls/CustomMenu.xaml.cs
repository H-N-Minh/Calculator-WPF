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
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class CustomMenu : UserControl
    {
        public static readonly DependencyProperty ExitCommandProperty =
            DependencyProperty.Register(
                nameof(ExitCommand),
                typeof(ICommand),
                typeof(CustomMenu),
                new PropertyMetadata(null));

        public ICommand ExitCommand
        {
            get => (ICommand)GetValue(ExitCommandProperty);
            set => SetValue(ExitCommandProperty, value);
        }

        public CustomMenu()
        {
            InitializeComponent();
        }
    }
}
