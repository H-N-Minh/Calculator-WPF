using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.Models;

public partial class Employee : ObservableObject
{
    [ObservableProperty]
    private int roleId;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int departmentId;
}

public partial class Role : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;
}

public partial class DepartmentNode : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    public ObservableCollection<DepartmentNode> Children { get; set; } = new();
}
