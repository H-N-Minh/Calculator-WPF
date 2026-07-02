using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.Models;

public class Employee
{
    public int RoleId { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }

}

public class Role
{
    int Id { get; set; }
    string Name { get; set; }
}

public partial class DepartmentNode : ObservableObject
{
    [ObservableProperty]
    private string name;

    public ObservableCollection<DepartmentNode> Children { get; set; } = new();
}
