using Library.Commands;
using Library.Models;
using Library.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

namespace YT;

public partial class MainWindow : Window
{
    
    // Ctor
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void Clear_Button_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Focus();
    }
}




