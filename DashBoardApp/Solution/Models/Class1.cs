using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.Models;

public class DepartmentNode
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<DepartmentNode> Children { get; set; } = new();
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Employee
{
    public string Name { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Department { get; set; } = string.Empty;
}

public class HardwareAsset
{
    public string AssetId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}

public partial class SoftwareLicense : ObservableObject
{
    [ObservableProperty]
    private string _softwareName = string.Empty;

    [ObservableProperty]
    private string _licenseKey = string.Empty;

    [ObservableProperty]
    private string _expirationDate = string.Empty;
}
