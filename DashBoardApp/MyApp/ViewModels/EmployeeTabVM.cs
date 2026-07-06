using CommunityToolkit.Mvvm.ComponentModel;
using DashBoardApp.Models;
using DashBoardApp.ViewModels.ModelsVM;
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

public partial class EmployeeTabVM : ObservableObject
{
    public ObservableCollection<EmployeeVM> AllEmployees { get; set; } = new();
    public ObservableCollection<DepartmentNodeVM> AllDepartments { get; set; } = new();
    public ObservableCollection<RoleVM> AllRoles { get; set; } = new();

    // Filtered list
    public ICollectionView VisibleEmployees { get; set; }

    [ObservableProperty]
    private HashSet<RoleVM> visibleRoles = new();

    // Selected Item
    [ObservableProperty]
    private DepartmentNodeVM? selectedDepartment = null;
    partial void OnSelectedDepartmentChanged(DepartmentNodeVM? value) => RefreshVisibleRoles();

    [ObservableProperty]
    private RoleVM? selectedRole;
    partial void OnSelectedRoleChanged(RoleVM? value) => VisibleEmployees.Refresh();
    
    // Ctor
    public EmployeeTabVM()
    {
        VisibleEmployees = CollectionViewSource.GetDefaultView(AllEmployees);
        VisibleEmployees.Filter = FilterEmployee;

        // Refresh the role list
        RefreshVisibleRoles();
    }

    // Method
    private void RefreshVisibleRoles()
    {
        VisibleRoles.Clear();

        // Add a default role
        RoleVM allRoleOption = new RoleVM(new Role { Id = -1, Name = "All Roles" });
        VisibleRoles.Add(allRoleOption);

        if (SelectedDepartment is null)
        {
            foreach (RoleVM role in AllRoles)
            {
                VisibleRoles.Add(role);
            }
        }
        else
        {
            VisibleRoles = SelectedDepartment.GetAllRoles();
        }

        SelectedRole = allRoleOption;
    }

    // Filter
    private bool FilterEmployee(object obj)
    {
        if (obj is not EmployeeVM emp) return false;

        bool matchingRoles = false;
        bool matchingDept = false;

        if (SelectedRole is not null && SelectedRole.Id != -1)
        {
            matchingRoles = SelectedRole.Id == emp.Role.Id;
        }
        else
        {
            matchingRoles = VisibleRoles.Any(r => r.Id == emp.Role.Id);
        }

        if (SelectedDepartment is null)
        {
            matchingDept = true;
        }
        else
        {
            matchingDept = SelectedDepartment.Id == emp.Department.Id;
        }

        return matchingRoles && matchingDept;
    }
}
