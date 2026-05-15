using SDK.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace SDK.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is AppViewModel vm)
            {
                vm.SearchCommand.Execute(null);
            }
        }
    }
}