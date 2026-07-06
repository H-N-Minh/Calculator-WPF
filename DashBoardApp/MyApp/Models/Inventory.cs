using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.ViewModels.ModelsVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace DashBoardApp.Models;

public partial class HardwareAsset : ObservableObject
{
    [ObservableProperty]
    private string assetId = string.Empty;

    [ObservableProperty]
    private string type = string.Empty;

    [ObservableProperty]
    private string manufacturer = string.Empty;

    [ObservableProperty]
    private EmployeeVM assignedTo;
}

