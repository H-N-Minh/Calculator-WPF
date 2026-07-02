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
    public partial class Menu : UserControl
    {
        public static readonly DependencyProperty ExitAppProperty =
            DependencyProperty.Register(
                nameof(ExitApp),
                typeof(ICommand),
                typeof(Menu),
                new PropertyMetadata(null));

        public ICommand ExitApp
        {
            get => (ICommand)GetValue(ExitAppProperty);
            set => SetValue(ExitAppProperty, value);
        }

        public Menu()
        {
            InitializeComponent();
        }
    }
}
