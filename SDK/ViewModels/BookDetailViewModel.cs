using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SDK.Commands;
using SDK.Models;

namespace SDK.ViewModels
{
    public enum BookDetailMode { View, Edit, Add }

    public class BookDetailViewModel : INotifyPropertyChanged
    {
        private readonly Book _book;
        private BookDetailMode _mode;

        public BookDetailMode Mode
        {
            get => _mode;
            set { _mode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsViewMode)); OnPropertyChanged(nameof(IsEditOrAddMode)); }
        }

        public bool IsViewMode => Mode == BookDetailMode.View;
        public bool IsEditOrAddMode => Mode == BookDetailMode.Edit || Mode == BookDetailMode.Add;
        public bool IsEditMode => Mode == BookDetailMode.Edit;

        // String wrappers for UI binding
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        private string _author = string.Empty;
        public string Author
        {
            get => _author;
            set { _author = value; OnPropertyChanged(); }
        }

        private string _publicationDate = string.Empty;
        public string PublicationDate
        {
            get => _publicationDate;
            set { _publicationDate = value; OnPropertyChanged(); }
        }

        private string _pages = string.Empty;
        public string Pages
        {
            get => _pages;
            set { _pages = value; OnPropertyChanged(); }
        }

        private string _price = string.Empty;
        public string Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private string _rating = string.Empty;
        public string Rating
        {
            get => _rating;
            set { _rating = value; OnPropertyChanged(); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public event Action<Book>? EditRequested;
        public event Action? BackRequested;
        public event Action<Book>? DeleteRequested;

        public Book? SavedBook { get; private set; }
        private Action? _closeDialog; // for modal windows

        public BookDetailViewModel(Book book, BookDetailMode mode, Action? closeDialog = null)
        {
            _book = book;
            Mode = mode;
            _closeDialog = closeDialog;

            LoadFromBook();

            SaveCommand = new RelayCommand(_ => Save(), _ => !string.IsNullOrWhiteSpace(Name));
            CancelCommand = new RelayCommand(_ => Cancel());
            BackCommand = new RelayCommand(_ => Back());
            EditCommand = new RelayCommand(_ => EditRequested?.Invoke(_book), _ => Mode == BookDetailMode.View);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => Mode == BookDetailMode.Edit);
        }

        private void LoadFromBook()
        {
            Name = _book.Name ?? string.Empty;
            Author = Mode == BookDetailMode.Edit && _book.Author == "(unknown)" ? string.Empty : (_book.Author ?? string.Empty);
            PublicationDate = _book.PublicationDate?.ToShortDateString() ?? string.Empty;
            Pages = _book.Pages?.ToString() ?? string.Empty;
            Price = _book.Price?.ToString("F2") ?? string.Empty;
            Rating = _book.Rating?.ToString("F1") ?? string.Empty;
            Description = _book.Description ?? string.Empty;

            // Replace empty strings with "(unknown)" for display in View mode
            if (Mode == BookDetailMode.View)
            {
                Author = string.IsNullOrEmpty(Author) ? "(unknown)" : Author;
                PublicationDate = string.IsNullOrEmpty(PublicationDate) ? "(unknown)" : PublicationDate;
                Pages = string.IsNullOrEmpty(Pages) ? "(unknown)" : Pages;
                Price = string.IsNullOrEmpty(Price) ? "(unknown)" : Price;
                Rating = string.IsNullOrEmpty(Rating) ? "(unknown)" : Rating;
            }
            else if (Mode == BookDetailMode.Edit)
            {
                // Clear "(unknown)" so user can type
                if (Author == "(unknown)") Author = string.Empty;
                if (PublicationDate == "(unknown)") PublicationDate = string.Empty;
                if (Pages == "(unknown)") Pages = string.Empty;
                if (Price == "(unknown)") Price = string.Empty;
                if (Rating == "(unknown)") Rating = string.Empty;
                // Description is left empty if it was empty (no "(unknown)")
            }
        }

        private void Save()
        {
            _book.Name = Name.Trim();

            // Author: if empty → "(unknown)"
            _book.Author = string.IsNullOrWhiteSpace(Author) ? "(unknown)" : Author.Trim();

            // Dates / numbers: if empty → null
            _book.PublicationDate = DateTime.TryParse(PublicationDate, out var d) ? d : null;
            _book.Pages = int.TryParse(Pages, out var p) ? p : null;
            _book.Price = decimal.TryParse(Price, out var pr) ? pr : null;
            _book.Rating = double.TryParse(Rating, out var r) ? r : null;
            _book.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

            SavedBook = _book;

            if (Mode == BookDetailMode.Add || Mode == BookDetailMode.Edit)
            {
                _closeDialog?.Invoke(); // close modal
            }
        }

        private void Cancel()
        {
            if (_closeDialog != null)
            {
                SavedBook = null;
                _closeDialog.Invoke();
            }
            else
            {
                Back();
            }
        }

        private void Back()
        {
            if (_closeDialog != null)
            {
                _closeDialog.Invoke();
            }
            else
            {
                BackRequested?.Invoke();
            }
        }

        private void Delete()
        {
            if (MessageBox.Show("Delete this book?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DeleteRequested?.Invoke(_book);
                BackRequested?.Invoke();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}