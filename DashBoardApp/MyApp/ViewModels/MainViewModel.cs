using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;
using System.Xml.Linq;

namespace DashBoardApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ViewModels
    public InventoryVM InventoryViewModel {  get; set; }
    public EmployeeTabVM EmployeeViewModel { get; set; } = new EmployeeTabVM();

    // Properties
    [ObservableProperty]
    private bool isLoading = false;

    // RelayCommands
    [RelayCommand]
    private void ExitApp() => System.Windows.Application.Current.Shutdown();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        await Task.Delay(2000);
        DataLoader.LoadDummyData(InventoryViewModel, EmployeeViewModel);
        IsLoading = false;
    }

    // Ctor
    public MainViewModel()
    {
        InventoryViewModel = new InventoryVM(EmployeeViewModel.AllEmployees);
        DataLoader.LoadDummyData(InventoryViewModel, EmployeeViewModel);
    }
}
