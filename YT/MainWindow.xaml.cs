using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

namespace YT;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    // Properties
    private string inputTextBox;
    public string InputTextBox
    {
        get { return inputTextBox; }
        set
        {
            inputTextBox = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand AddCommand { get; set; }
    public RelayCommand DeleteCommand { get; set; }
    public RelayCommand ClearAllCommand { get; set; }

    public ObservableCollection<Book> Books { get; set; }

    // Events
    public event PropertyChangedEventHandler? PropertyChanged;  

    // Ctor
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        AddCommand = new RelayCommand(AddBook);
        DeleteCommand = new RelayCommand(DeleteBook);
        ClearAllCommand = new RelayCommand(ClearAllBooks);
        Books = new ObservableCollection<Book>
        {
            new Book { Author = "Author 1", Title = "Title 1", PageCount = 100 },
            new Book { Author = "Author 2", Title = "Title 2", PageCount = 200 },
            new Book { Author = "Author 3", Title = "Title 3", PageCount = 300 }
        };
    }

    // Event Handlers
    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (InputTextBox?.Length > 0)
        {
            UITextBlock.Visibility = Visibility.Hidden;
        }
        else
        {
            UITextBlock.Visibility = Visibility.Visible;
            
        }
    }

    // Methods
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void AddBook(object? parameter)
    {
        string bookName = (string) (parameter ?? "");
        if (bookName == "")
        {
            MessageBoxResult result = MessageBox.Show("Cant add a book with no name", "Name missing?", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Books.Add(new Book { Author = "New Author", Title = bookName, PageCount = 0 });
        InputTextBox = "";
    }

    private void DeleteBook(object? parameter)
    {
        if (parameter == null)
        {
            MessageBox.Show("You must select an item first in order to delete.", "Sure?", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        var result = MessageBox.Show("Are you sure u wanna delete this book?", "Sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes && parameter is Book book)
        {
            Books.Remove(book);
        }
    }

    private void ClearAllBooks(object? parameter)
    {
        var result = MessageBox.Show("Are you sure u wanna delete all books?", "Sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            Books.Clear();
        }
    }
}

