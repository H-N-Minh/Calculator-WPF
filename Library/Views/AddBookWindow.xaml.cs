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
        private ViewModels.ViewModelBase viewModel;
        public AddBookWindow()
        {
            InitializeComponent();
            viewModel = new ViewModels.AddBookVM();
            DataContext = viewModel;
        }
    }
}
