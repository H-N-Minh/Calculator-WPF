using System.ComponentModel;
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

namespace Panels
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private A a = new A();
        private B b = new B();

        public object? Screen { get; set { field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Screen))); } }
        public MainWindow()
        {

            InitializeComponent();

            DataContext = this;
            Screen = a;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Screen == a)
                Screen = b;
            else
                Screen = a;

        }
    }

    public class A
    {

    }

    public class B
    {

    }
}