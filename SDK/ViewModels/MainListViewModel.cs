using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SDK.Commands;
using SDK.Models;

namespace SDK.ViewModels
{
    public class MainListViewModel : INotifyPropertyChanged
    {
        private readonly AppViewModel _app;

        public ObservableCollection<Book> Books => _app.FilteredBooks;

        // Multi‑select synchronised by an attached behaviour
        public ObservableCollection<Book> SelectedBooks { get; } = new();

        public ICommand ViewBookCommand { get; }
        public ICommand AddBookCommand { get; }
        public ICommand RemoveBooksCommand { get; }

        public MainListViewModel(AppViewModel app)
        {
            _app = app;
            ViewBookCommand = new RelayCommand(book => _app.Screen = new BookDetailViewModel((Book)book!, BookDetailMode.View));
            AddBookCommand = new RelayCommand(_ => _app.AddBook());
            RemoveBooksCommand = new RelayCommand(
                _ => _app.RemoveBooks(SelectedBooks),
                _ => SelectedBooks.Count > 0);

            SelectedBooks.CollectionChanged += (_, _) => ((RelayCommand)RemoveBooksCommand).RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}