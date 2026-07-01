using PriorityTaskBoard.ViewModels;
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

namespace PriorityTaskBoard.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel vm = new MainViewModel();
            DataContext = vm;
        }

        // Handled in View code-behind because changing theme is purely a UI concern.
        private void LightTheme_Click(object sender, RoutedEventArgs e) => App.ChangeTheme("Light");
        private void DarkTheme_Click(object sender, RoutedEventArgs e) => App.ChangeTheme("Dark");
    }
}