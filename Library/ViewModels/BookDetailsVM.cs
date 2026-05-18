using System;
using System.Collections.Generic;
using System.Text;
using Library.Commands;
using Library.Models;

namespace Library.ViewModels;

public class BookDetailsVM : ViewModelBase
{

    public Book Book { get; }

    public RelayCommand ReturnToListView { get; }
    public BookDetailsVM(Book book, ViewModelBase previousScreen, Action<ViewModelBase> switchScreenTo)
    {
        this.Book = book;
        ReturnToListView = new RelayCommand((_) => switchScreenTo(previousScreen));
    }
}
