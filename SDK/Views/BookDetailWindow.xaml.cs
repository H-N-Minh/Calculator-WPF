using SDK.ViewModels;
using SDK.Models;
using SDK.ViewModels;
using System.Windows;

namespace SDK.Views
{
    public partial class BookDetailWindow : Window
    {
        private readonly BookDetailViewModel _vm;

        public BookDetailWindow(BookDetailViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;

            // Provide the close action to the ViewModel
            var closeAction = new Action(() =>
            {
                if (_vm.SavedBook != null)
                    DialogResult = true;
                else
                    DialogResult = false;
                Close();
            });

            // Recreate the ViewModel with close action (a bit hacky, but keeps it simple)
            var newVm = new BookDetailViewModel(viewModel.SavedBook ?? new Book(), viewModel.Mode, closeAction);
            DataContext = newVm;
        }
    }
}