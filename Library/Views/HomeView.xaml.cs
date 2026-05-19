using Library.Models;
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

namespace Library.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void Add_Book_Click(object sender, RoutedEventArgs e)
        {
            AddBookWindow addBookWindow = new AddBookWindow();
            addBookWindow.Owner = Window.GetWindow(this);
            bool? result = addBookWindow.ShowDialog();

            if (result == true && DataContext is ViewModels.HomeVM homeVM)
            {
                homeVM.AddBook(addBookWindow.ViewModel);
            }
        }
    }
}
