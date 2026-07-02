using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;

namespace DashBoardApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<Employee> AllEmployees { get; set; } = new();
    public ObservableCollection<HardwareAsset> Hardwares { get; set; } = new();

    public MainViewModel()
    {
        LoadDummyData();
    }

    private void LoadDummyData()
    {
        // 3. Setup ListBox Data (Expanded Employee Roster)
        AllEmployees.Clear();
        AllEmployees.Add(new Employee { Name = "Sarah Jenkins", RoleId = 1, Department = "Enterprise IT Division" });
        AllEmployees.Add(new Employee { Name = "David Chen", RoleId = 2, Department = "Backend Team" });
        AllEmployees.Add(new Employee { Name = "Elena Rostova", RoleId = 3, Department = "Frontend Team" });
        AllEmployees.Add(new Employee { Name = "Marcus Aurelius", RoleId = 4, Department = "Backend Team" });
        AllEmployees.Add(new Employee { Name = "Aisha Rahman", RoleId = 2, Department = "Cloud & DevOps" });
        AllEmployees.Add(new Employee { Name = "John Doe", RoleId = 4, Department = "Cloud & DevOps" });
        AllEmployees.Add(new Employee { Name = "Karen Smith", RoleId = 5, Department = "Cybersecurity" });
        AllEmployees.Add(new Employee { Name = "Liam O'Connor", RoleId = 3, Department = "Network Administration" });
        AllEmployees.Add(new Employee { Name = "Sanjay Patel", RoleId = 2, Department = "Tier 1 Helpdesk" });
        AllEmployees.Add(new Employee { Name = "Chloe Dupont", RoleId = 6, Department = "Tier 1 Helpdesk" });
        AllEmployees.Add(new Employee { Name = "Yuki Tanaka", RoleId = 6, Department = "Tier 2 Desktop Support" });
        AllEmployees.Add(new Employee { Name = "Oliver Hansen", RoleId = 4, Department = "QA & Testing" });


        Hardwares.Clear();
        Hardwares.Add(new HardwareAsset { AssetId = "LT-9921", Type = "MacBook Pro M3", Manufacturer = "Apple", AssignedTo = AllEmployees[0] });
        Hardwares.Add(new HardwareAsset { AssetId = "LT-4412", Type = "ThinkPad P1 Gen 6", Manufacturer = "Lenovo", AssignedTo = AllEmployees[1] });
        Hardwares.Add(new HardwareAsset { AssetId = "SRV-8801", Type = "PowerEdge R760 Server", Manufacturer = "Dell", AssignedTo = AllEmployees[2] });
        Hardwares.Add(new HardwareAsset { AssetId = "SW-2209", Type = "Catalyst 9300 Switch", Manufacturer = "Cisco", AssignedTo = AllEmployees[3] });
        Hardwares.Add(new HardwareAsset { AssetId = "LT-1029", Type = "Latitude 5440", Manufacturer = "Dell", AssignedTo = AllEmployees[4] });
        Hardwares.Add(new HardwareAsset { AssetId = "MN-5541", Type = "UltraSharp 34\" Curved", Manufacturer = "Dell", AssignedTo = AllEmployees[5] });
        Hardwares.Add(new HardwareAsset { AssetId = "PH-3310", Type = "IP Phone 8841", Manufacturer = "Cisco", AssignedTo = AllEmployees[6] });
    }
}
