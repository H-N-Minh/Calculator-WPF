using Library.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace YT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Window window = new MainWindow();
            window.DataContext = new Library.ViewModels.MainViewModel();
            window.Show();
        }
    }

}
