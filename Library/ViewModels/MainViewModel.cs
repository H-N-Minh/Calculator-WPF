using Library.Commands;
using Library.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Library.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    // Fields
    private readonly Window mainWindow;

    // Properties
    public ObservableCollection<Book> AllBooks { get; set; } = new();

    // Ctor
    public MainViewModel(Window mainWindow)
    {
        this.mainWindow = mainWindow;
        AllBooks.Add(new Book(title: "The Hobbit", author: "J.R.R. Tolkien", publishDate: new DateTime(1937, 9, 21), pages: 310));
        AllBooks.Add(new Book(title: "1984", author: "George Orwell", pages: 328));
        AllBooks.Add(new Book(title: "Clean Code"));

    }

    // Events
    public event PropertyChangedEventHandler? PropertyChanged;

    // Methods
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
