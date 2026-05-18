using Library.Commands;
using Library.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Library.ViewModels;

class BooksListVM : ViewModelBase
{

    private readonly ObservableCollection<Book> allBooks = new();
    private readonly Action<ViewModelBase> switchScreenTo;

    // Binded Properties
    private string searchQuery = string.Empty;
    public string SearchQuery
    {
        get { return searchQuery; }
        set
        {
            searchQuery = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPlaceHolderVisible));        // Notify the placeholder to change visibility
            VisibleBooks.Refresh();                                 // Notify the listview to re-apply the filter
            OnPropertyChanged(nameof(VisibleBooksCount));           // Notify the new listview count to update
            ClearSearchQueryCMD.RaiseCanExecuteChanged();           // Notify the X button to update its enabled state
        }
    }

    public Visibility IsPlaceHolderVisible => string.IsNullOrEmpty(SearchQuery) ? Visibility.Visible : Visibility.Hidden;


    public ICollectionView VisibleBooks { get; }

    public int VisibleBooksCount { get { return VisibleBooks.Cast<Book>().Count(); } }

    // Commands
    public RelayCommand ClearSearchQueryCMD { get; }
    public RelayCommand RemoveBookCMD { get; }

    // Ctor
    public BooksListVM(Action<ViewModelBase> switchScreenTo)
    {
        this.switchScreenTo = switchScreenTo;
        // Set the view and filter
        VisibleBooks = CollectionViewSource.GetDefaultView(allBooks);
        VisibleBooks.Filter = FilterBooks;

        // Bind the commands
        ClearSearchQueryCMD = new RelayCommand((_) => SearchQuery = string.Empty, (_) => !string.IsNullOrEmpty(SearchQuery));
        RemoveBookCMD = new RelayCommand(RemoveSelectedBook);

        // Load initial data
        LoadBooks();
    }


    // Methods
    private void RemoveSelectedBook(object? parameter)
    {
        if (parameter is System.Collections.IList selectedBooks && selectedBooks.Count > 0)
        {
            var booksToRemove = selectedBooks.Cast<Book>().ToList();
            foreach (var book in booksToRemove)
            {
                allBooks.Remove(book);
            }
            OnPropertyChanged(nameof(VisibleBooksCount));
        }
    }

    private void LoadBooks()
    {
        var books = new List<Book>
        {
            new("The Hobbit", "J.R.R. Tolkien"),
            new("1984"), // Testing (unknown)
            new("Clean Code", "Robert C. Martin"),
            new("The Great Gatsby", "F. Scott Fitzgerald"),
            new("Moby Dick"),
            new("Brave New World", "Aldous Huxley"),
            new("The Catcher in the Rye"),
            new("War and Peace", "Leo Tolstoy"),
            new("Ulysses"),
            new("The Divine Comedy", "Dante Alighieri"),
            new("The Great Gatsby", "F. Scott Fitzgerald", new DateTime(1925, 4, 10), 218, 10.99m, 4.2f, "A novel about the American dream."),
            new("To Kill a Mockingbird", "Harper Lee", new DateTime(1960, 7, 11), 281, 7.99m, 4.8f, "A novel about racial injustice in the Deep South."),
            new("1984", "George Orwell", new DateTime(1949, 6, 8), 328, 9.99m, 4.6f, "A dystopian novel about totalitarianism."),
            new("Pride and Prejudice", "Jane Austen", new DateTime(1813, 1, 28), 279, 6.99m, 4.5f, "A romantic novel about manners and marriage."),
            new("The Catcher in the Rye", "J.D. Salinger", new DateTime(1951, 7, 16), 214, 8.99m, 4.0f, "A novel about teenage rebellion and alienation."),
            new("The Fellowship of the Ring", "J.R.R. Tolkien"),
            new("Crime and Punishment"),
            new("Dune", "Frank Herbert"),
            new("Frankenstein", "Mary Shelley"),
            new("The Hobbit"),
            new("Fahrenheit 451", "Ray Bradbury"),
            new("The Little Prince"),
            new("Wuthering Heights", "Emily Brontë"),
            new("Dracula"),
            new("The Picture of Dorian Gray", "Oscar Wilde"),
            new("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 604, 14.99m, 4.7f, "A masterpiece of science fiction set on the desert planet Arrakis."),
            new("Frankenstein", "Mary Shelley", new DateTime(1818, 1, 1), 280, 5.99m, 4.3f, "A gothic novel exploring the consequences of playing God."),
            new("Fahrenheit 451", "Ray Bradbury", new DateTime(1953, 10, 19), 158, 8.99m, 4.6f, "A dystopian novel depicting a future society where books are banned and burned."),
            new("The Picture of Dorian Gray", "Oscar Wilde", new DateTime(1890, 7, 1), 250, 6.99m, 4.4f, "A philosophical novel about youth, vanity, and moral corruption."),
            new("The Hobbit", "J.R.R. Tolkien", new DateTime(1937, 9, 21), 310, 10.99m, 4.8f, "A classic fantasy novel following the adventures of Bilbo Baggins.")

        };

        foreach (var book in books)
        {
            allBooks.Add(book);
        }
        OnPropertyChanged(nameof(VisibleBooksCount));
    }

    private bool FilterBooks(object obj)
    {
        if (obj is not Book book) return false;

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            return true;
        }

        return book.DisplayInfo.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
    }
}
