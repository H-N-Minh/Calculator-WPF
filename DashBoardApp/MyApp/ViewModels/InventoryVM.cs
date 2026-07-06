using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.Models;
using DashBoardApp.ViewModels.ModelsVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.ViewModels;

public class InventoryVM : ObservableObject
{
    public ObservableCollection<HardwareAsset> Hardwares { get; set; } = new();
    public ObservableCollection<EmployeeVM> AllEmployees { get; set; }

    public InventoryVM(ObservableCollection<EmployeeVM> allEmployees)
    {
        AllEmployees = allEmployees;
    }
}
