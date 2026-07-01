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
using System.Windows.Data;

namespace Library.ViewModels;


public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MainViewModel : ViewModelBase
{
    private ViewModelBase screen;
    public ViewModelBase Screen
    {
        get => screen;
        set { screen = value; OnPropertyChanged(); }
    }

    public MainViewModel()
    {
        Screen = new HomeVM(SwitchScreenTo);
    }

    private void SwitchScreenTo(ViewModelBase screen)
    {
        Screen = screen;
    }
}
