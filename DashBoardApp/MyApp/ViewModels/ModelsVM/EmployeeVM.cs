using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DashBoardApp.ViewModels.ModelsVM;

public partial class EmployeeVM : ObservableObject
{
    [ObservableProperty]
    private RoleVM? role;

    [ObservableProperty]
    private DepartmentNodeVM? department;

    [ObservableProperty]
    private string name = string.Empty;

    public EmployeeVM(Employee emp)
    {
        Name = emp.Name;
        
        if (RoleVM.AllRoles.TryGetValue(emp.RoleId, out RoleVM? role))
        {
            Role = role;
        }

        if (DepartmentNodeVM.AllDepartments.TryGetValue(emp.DepartmentId, out var dept))
        {
            Department = dept;
        }
    }
}

public partial class RoleVM : ObservableObject
{
    public static Dictionary<int, RoleVM> AllRoles = new();

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name;

    public RoleVM(Role role)
    {
        Id = role.Id;
        Name = role.Name;
        AllRoles[Id] = this;
    }
}

public partial class DepartmentNodeVM : ObservableObject
{
    public static Dictionary<int, DepartmentNodeVM> AllDepartments = new();

    public HashSet<RoleVM> Roles { get; set; } = new();

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name;

    public ObservableCollection<DepartmentNodeVM> Children { get; set; } = new();

    public DepartmentNodeVM(DepartmentNode dep)
    {
        Id = dep.Id;
        Name = dep.Name;
        AllDepartments[Id] = this;

        foreach (var child in dep.Children)
        {
            if (AllDepartments.TryGetValue(child.Id, out var depVM))
            {
                Children.Add(depVM);
            }
            else
            {
                Children.Add(new DepartmentNodeVM(child));
            }
        }
    }

    public HashSet<RoleVM> GetAllRoles()
    {
        HashSet<RoleVM> allRoles = new();

        foreach (var role in Roles)
        {
            allRoles.Add(role);
        }

        foreach (var child in Children)
        {
            foreach (var role in child.GetAllRoles())
            {
                allRoles.Add(role);
            }
        }

        return allRoles;
    }
}


