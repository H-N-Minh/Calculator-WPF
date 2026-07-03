using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;

namespace DashBoardApp.ViewModels;

public partial class EmployeeVM : ObservableObject
{
    public ObservableCollection<Employee> AllEmployees { get; set; } = new();
    public ObservableCollection<DepartmentNode> AllDepartments { get; set; } = new();
    public ObservableCollection<Role> AllRoles { get; set; } = new();

    // Roles map
    public Dictionary<int, List<int>> RoleDeparmentMap = new();

    // Filtered list
    public ICollectionView VisibleEmployees { get; set; }
    public ICollectionView VisibleRoles { get; set; }

    // Selected Item
    [ObservableProperty]
    private DepartmentNode? selectedDepartment;
    partial void OnSelectedDepartmentChanged(DepartmentNode? value) => VisibleRoles.Refresh();

    [ObservableProperty]
    private Role? selectedRole;
    partial void OnSelectedRoleChanged(Role? value) => VisibleEmployees.Refresh();
    
    // Ctor
    public EmployeeVM()
    {
        VisibleEmployees = CollectionViewSource.GetDefaultView(AllEmployees);
        VisibleEmployees.Filter = FilterEmployee;

        VisibleRoles = CollectionViewSource.GetDefaultView(AllRoles);
        VisibleRoles.Filter = FilterRole;

        // Update the map whenever theres a change
        AllEmployees.CollectionChanged += RefreshRoleDepartmentMap;
        AllDepartments.CollectionChanged += RefreshRoleDepartmentMap;
        AllRoles.CollectionChanged += RefreshRoleDepartmentMap;
    }

    // Method
    private void RefreshRoleDepartmentMap(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RoleDeparmentMap.Clear();

        foreach (Employee emp in AllEmployees)
        {
            int role = emp.RoleId;
            int department = emp.DepartmentId;

            ref var roleList = ref CollectionsMarshal.GetValueRefOrAddDefault<int, List<int>>(RoleDeparmentMap, department, out _);

            roleList ??= new List<int>();

            roleList.Add(role);
        }
    }

    // Filter
    private bool FilterRole(object obj)
    {
        if (obj is not Role role) return false;
        if (SelectedDepartment is null) return true;

        List<int>? validRoles = RoleDeparmentMap.GetValueOrDefault(SelectedDepartment.Id);

        return validRoles?.Contains(role.Id) ?? false;
    }

    private bool FilterEmployee(object obj)
    {
        if (obj is not Employee emp) return false;

        if (SelectedRole is not null) return SelectedRole.Id == emp.RoleId;

        VisibleRoles.Refresh();

        return VisibleRoles.Cast<Role>().Any(r => r.Id == emp.RoleId);
    }
}
