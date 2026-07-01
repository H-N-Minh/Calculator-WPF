using Library.Commands;
using Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.ViewModels;

public class AddBookVM : ViewModelBase
{
    public Book? NewBook {  get; set; }
    private string? title;

    public string? Title
    {
        get { return title; }
        set { title = value; OnPropertyChanged(); SaveBookCMD.RaiseCanExecuteChanged(); }
    }

    public string? Author { get; set; }
    public DateTime? PublishDate { get; set; }
    public int? Pages { get; set; }
    public decimal? Price { get; set; }
    public float? Rating { get; set; }
    public string? Description { get; set; }

    // Commands
    public RelayCommand SaveBookCMD { get;  }
    public RelayCommand CancelCMD { get;  }

    // Delegate
    private Action<bool> CloseWindow;

    // Ctor
    public AddBookVM(Action<bool> CloseWindow)
    {
        SaveBookCMD = new RelayCommand(SaveBook, _ => !string.IsNullOrEmpty(Title));
        CancelCMD = new RelayCommand(_ => CloseWindow(false));
        this.CloseWindow = CloseWindow;
    }

    public void SaveBook(object? parameter)
    {
        string title = string.IsNullOrEmpty(Title) ? "" : Title;
        NewBook = new Book(title, Author, PublishDate, Pages, Price, Rating, Description);
        CloseWindow(true);
    }
}
