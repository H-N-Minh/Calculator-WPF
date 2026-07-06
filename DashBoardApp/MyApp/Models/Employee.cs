using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.Models;

public class Employee
{
    public int RoleId { get; set; }

    public int DepartmentId {  get; set; }

    public string Name { get; set; } = string.Empty;
}

public class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class DepartmentNode
{
    public int Id;

    public string Name { get; set; } = string.Empty;

    public List<DepartmentNode> Children { get; set; } = new();
}
