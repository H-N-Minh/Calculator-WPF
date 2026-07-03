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
using System.Xml.Linq;

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
    public ObservableCollection<Role> VisibleRoles { get; set; } = new();

    // Selected Item
    [ObservableProperty]
    private DepartmentNode? selectedDepartment = null;
    partial void OnSelectedDepartmentChanged(DepartmentNode? value)
    {
        VisibleRoles.Clear();
        HashSet<int> validRolesId = new();

        if (value is null)
        {
            VisibleRoles = new ObservableCollection<Role>(AllRoles);
        }
        else
        {
            AccumulateRoleIdForSubTree(value);
            var filteredRoles = AllRoles.Where(role => validRolesId.Contains(role.Id));
            foreach (Role role in filteredRoles)
            {
                VisibleRoles.Add(role);
            }
        }

        void AccumulateRoleIdForSubTree(DepartmentNode node)
        {
            if (RoleDeparmentMap.TryGetValue(node.Id, out var roles))
            {
                foreach (var roleId in roles)
                {
                    validRolesId.Add(roleId); // HashSet automatically ignores duplicates
                }
            }

            foreach (var child in node.Children)
            {
                AccumulateRoleIdForSubTree(child);
            }
        }
    }

    [ObservableProperty]
    private Role? selectedRole;
    partial void OnSelectedRoleChanged(Role? value) => VisibleEmployees.Refresh();
    
    // Ctor
    public EmployeeVM()
    {
        VisibleEmployees = CollectionViewSource.GetDefaultView(AllEmployees);
        VisibleEmployees.Filter = FilterEmployee;

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
    private bool FilterEmployee(object obj)
    {
        if (obj is not Employee emp) return false;

        if (SelectedRole is not null) return SelectedRole.Id == emp.RoleId;

        return VisibleRoles.Any(r => r.Id == emp.RoleId);
    }
}
