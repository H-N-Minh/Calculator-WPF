using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;
using System.Xml.Linq;

namespace DashBoardApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<Employee> AllEmployees { get; set; } = new();
    public ICollectionView VisibleEmployees { get; set; }
    public ObservableCollection<HardwareAsset> Hardwares { get; set; } = new();
    public ObservableCollection<DepartmentNode> AllDepartments { get; set; } = new();

    [ObservableProperty]
    private DepartmentNode selectedDepartment;

    [ObservableProperty]
    private bool isLoading = false;

    [RelayCommand]
    private void ExitApp()
    {
        System.Windows.Application.Current.Shutdown();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;

        await Task.Delay(2000);

        LoadDummyData();

        IsLoading = false;
    }

    public MainViewModel()
    {
        LoadDummyData();
        VisibleEmployees = CollectionViewSource.GetDefaultView(AllEmployees);
        VisibleEmployees.Filter = FilterEmployee;
    }

    private bool FilterEmployee(object obj)
    {
        if (obj is not Employee emp) return false;

        return true;
    }
    private void LoadDummyData()
    {
        // 1. Setup TreeView Data (Deep Hierarchical Structure)
        AllDepartments.Clear();

        var devDept = new DepartmentNode { Name = "Software Engineering" };
        devDept.Children.Add(new DepartmentNode { Name = "Frontend Team" });
        devDept.Children.Add(new DepartmentNode { Name = "Backend Team" });
        devDept.Children.Add(new DepartmentNode { Name = "QA & Testing" });

        var opsDept = new DepartmentNode { Name = "IT Operations" };
        opsDept.Children.Add(new DepartmentNode { Name = "Cloud & DevOps" });
        opsDept.Children.Add(new DepartmentNode { Name = "Cybersecurity" });
        opsDept.Children.Add(new DepartmentNode { Name = "Network Administration" });

        var supportDept = new DepartmentNode { Name = "Support Services" };
        supportDept.Children.Add(new DepartmentNode { Name = "Tier 1 Helpdesk" });
        supportDept.Children.Add(new DepartmentNode { Name = "Tier 2 Desktop Support" });

        var corporateIT = new DepartmentNode { Name = "Enterprise IT Division" };
        corporateIT.Children.Add(devDept);
        corporateIT.Children.Add(opsDept);
        corporateIT.Children.Add(supportDept);

        AllDepartments.Add(corporateIT);

        #region Second Child Branch: Digital Marketing & Operations
        var creativeDept = new DepartmentNode { Name = "Creative & Design" };
        creativeDept.Children.Add(new DepartmentNode { Name = "UI/UX Design" });
        creativeDept.Children.Add(new DepartmentNode { Name = "Content Creation" });

        var analyticsDept = new DepartmentNode { Name = "Data & Analytics" };
        analyticsDept.Children.Add(new DepartmentNode { Name = "SEO & Growth" });
        analyticsDept.Children.Add(new DepartmentNode { Name = "Business Intelligence" });

        var marketingDept = new DepartmentNode { Name = "Marketing Operations" };
        marketingDept.Children.Add(new DepartmentNode { Name = "Social Media" });
        marketingDept.Children.Add(new DepartmentNode { Name = "Paid Advertising" });

        var digitalMarketing = new DepartmentNode { Name = "Digital Marketing Division" };
        digitalMarketing.Children.Add(creativeDept);
        digitalMarketing.Children.Add(analyticsDept);
        digitalMarketing.Children.Add(marketingDept);

        AllDepartments.Add(digitalMarketing);
        #endregion



        // 2. Setup ComboBox Data (Granular Roles)
        //Roles.Clear();
        //Roles.Add(new Role { Id = 0, Name = "All Roles" });
        //Roles.Add(new Role { Id = 1, Name = "Director / VP" });
        //Roles.Add(new Role { Id = 2, Name = "Team Lead" });
        //Roles.Add(new Role { Id = 3, Name = "Senior Engineer" });
        //Roles.Add(new Role { Id = 4, Name = "Engineer" });
        //Roles.Add(new Role { Id = 5, Name = "Security Analyst" });
        //Roles.Add(new Role { Id = 6, Name = "Support Specialist" });


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
