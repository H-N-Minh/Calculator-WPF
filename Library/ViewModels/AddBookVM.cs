using Library.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.ViewModels;

public class AddBookVM : ViewModelBase
{
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
    public RelayCommand SaveBookCMD { get; set; }
    public RelayCommand CancelCMD { get; set; }

    // Ctor
    public AddBookVM()
    {
        SaveBookCMD = new RelayCommand(SaveBook, _ => !string.IsNullOrEmpty(Title));

    }

    public void SaveBook(object? parameter)
    {

    }
}
