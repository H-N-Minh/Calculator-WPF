using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SDK.Commands;
using SDK.Models;
using SDK.Views;

namespace SDK.ViewModels
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Book> _allBooks = new();

        public ObservableCollection<Book> FilteredBooks { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        private object? _screen;
        public object? Screen
        {
            get => _screen;
            set { _screen = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public AppViewModel()
        {
            // sample data
            _allBooks.Add(new Book { Name = "The Hobbit", Author = "J.R.R. Tolkien", Pages = 310, PublicationDate = new DateTime(1937, 9, 21) });
            _allBooks.Add(new Book { Name = "1984", Author = "George Orwell", Pages = 328 });
            _allBooks.Add(new Book { Name = "Clean Code", Author = "Robert C. Martin", Pages = 464 });

            ShowAllBooks();

            SearchCommand = new RelayCommand(_ => Search());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
        }

        private void ShowAllBooks()
        {
            FilteredBooks.Clear();
            foreach (var b in _allBooks)
                FilteredBooks.Add(b);

            Screen = new MainListViewModel(this);
        }

        private void Search()
        {
            var search = SearchText?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(search))
            {
                ShowAllBooks();
                return;
            }

            var filtered = _allBooks.Where(b =>
            {
                var display = $"{b.Name} - {(b.Author ?? "(unknown)")}";
                return display.Contains(search, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            FilteredBooks.Clear();
            foreach (var b in filtered)
                FilteredBooks.Add(b);

            if (filtered.Count == 1)
            {
                // Single result → show detail in the ContentPresenter
                var detailVm = new BookDetailViewModel(filtered[0], BookDetailMode.View);
                detailVm.EditRequested += OnEditRequested;
                detailVm.BackRequested += () => ShowList();
                detailVm.DeleteRequested += book => DeleteBook(book);
                Screen = detailVm;
            }
            else
            {
                ShowList(); // remains in list view, FilteredBooks already updated
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            ShowAllBooks();
        }

        public void ShowList()
        {
            Screen = new MainListViewModel(this);
        }

        public void AddBook()
        {
            var newBook = new Book();
            var vm = new BookDetailViewModel(newBook, BookDetailMode.Add);
            var win = new BookDetailWindow(vm);
            if (win.ShowDialog() == true)
            {
                _allBooks.Add(vm.SavedBook!);
                Search(); // refresh list / auto‑navigate
            }
        }

        private void OnEditRequested(Book book)
        {
            // Clone the book to avoid modifying the original until save
            var clone = new Book
            {
                Name = book.Name,
                Author = book.Author,
                PublicationDate = book.PublicationDate,
                Pages = book.Pages,
                Price = book.Price,
                Rating = book.Rating,
                Description = book.Description
            };
            var vm = new BookDetailViewModel(clone, BookDetailMode.Edit);
            var win = new BookDetailWindow(vm);
            if (win.ShowDialog() == true)
            {
                var saved = vm.SavedBook!;
                var original = _allBooks.First(b => b.Name == saved.Name && b.Author == saved.Author); // better to keep reference
                var idx = _allBooks.IndexOf(original);
                if (idx >= 0)
                {
                    _allBooks[idx] = saved;
                }
                // Refresh search and show the saved book in view mode
                Search();
            }
            else
            {
                // cancelled: stay on the current detail view
            }
        }

        public void DeleteBook(Book book)
        {
            if (MessageBox.Show("Delete this book?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _allBooks.Remove(book);
                Search(); // refresh
            }
        }

        // Called by MainListViewModel.RemoveBooksCommand
        public void RemoveBooks(IEnumerable<Book> books)
        {
            var list = books.ToList();
            if (list.Count == 0) return;
            if (MessageBox.Show($"Are you sure you want to remove {list.Count} item(s)?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (var b in list)
                    _allBooks.Remove(b);
                Search();
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}