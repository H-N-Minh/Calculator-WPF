using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.ViewModels;

public class InventoryVM : ObservableObject
{
    public ObservableCollection<HardwareAsset> Hardwares { get; set; } = new();
}
