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

namespace ScreenApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public object? Screen { get; set; }

    private A a;
    private B b;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        a = new A();
        b = new B();
        Screen = a;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Screen = Screen switch
        {
            A => b,
            B => a
        };
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Screen)));
    }
}

public class A
{
    
}

public class B
{

}