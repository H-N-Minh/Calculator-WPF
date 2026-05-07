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
            DataForMainWindow dataWindow = new DataForMainWindow();
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = dataWindow;
            mainWindow.Show();
        }
    }

}
