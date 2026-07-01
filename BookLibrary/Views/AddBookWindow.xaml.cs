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
using System.Windows.Shapes;

namespace Library.Views
{
    /// <summary>
    /// Interaction logic for AddBookWindow.xaml
    /// </summary>
    public partial class AddBookWindow : Window
    {
        public ViewModels.AddBookVM ViewModel { get; set; }
        public AddBookWindow()
        {
            InitializeComponent();
            ViewModel = new ViewModels.AddBookVM(CloseWindow);
            DataContext = ViewModel;
        }

        private void CloseWindow(bool result)
        {
            this.DialogResult = result;
            this.Close();
        }
    }
}
