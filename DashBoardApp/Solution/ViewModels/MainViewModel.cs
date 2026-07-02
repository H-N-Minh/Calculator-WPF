using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DashBoardApp.Models;
using System.Collections.ObjectModel;

namespace DashBoardApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusBarText = "Ready...";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedRoleId;

    public ObservableCollection<DepartmentNode> Departments { get; } = new();
    public ObservableCollection<Role> Roles { get; } = new();

    private readonly ObservableCollection<Employee> _allEmployees = new();
    public ObservableCollection<Employee> FilteredEmployees { get; } = new();

    public ObservableCollection<HardwareAsset> HardwareAssets { get; } = new();
    public ObservableCollection<SoftwareLicense> SoftwareLicenses { get; } = new();

    public MainViewModel()
    {
        // Load initial dummy data on startup
        _ = RefreshDataAsync();
    }

    // Runs when SelectedRoleId changes via the ComboBox
    partial void OnSelectedRoleIdChanged(int value)
    {
        FilteredEmployees.Clear();
        var filtered = value == 0 ? _allEmployees : _allEmployees.Where(e => e.RoleId == value);

        foreach (var emp in filtered)
        {
            FilteredEmployees.Add(emp);
        }
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        IsLoading = true;
        StatusBarText = "Loading data...";

        // Simulate network call
        await Task.Delay(2000);

        LoadDummyData();

        IsLoading = false;
        StatusBarText = "Ready...";
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void LoadDummyData()
    {
        // 1. Setup TreeView Data (Deep Hierarchical Structure)
        Departments.Clear();

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

        Departments.Add(corporateIT);

        // 2. Setup ComboBox Data (Granular Roles)
        Roles.Clear();
        Roles.Add(new Role { Id = 0, Name = "All Roles" });
        Roles.Add(new Role { Id = 1, Name = "Director / VP" });
        Roles.Add(new Role { Id = 2, Name = "Team Lead" });
        Roles.Add(new Role { Id = 3, Name = "Senior Engineer" });
        Roles.Add(new Role { Id = 4, Name = "Engineer" });
        Roles.Add(new Role { Id = 5, Name = "Security Analyst" });
        Roles.Add(new Role { Id = 6, Name = "Support Specialist" });


        // Trigger filter refresh
        SelectedRoleId = 0;
        OnSelectedRoleIdChanged(0);

        // 5. Setup DataGrid Data (Expanded Software Licensing)
        SoftwareLicenses.Clear();
        SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "Microsoft 365 Enterprise", LicenseKey = "E5-M365-XXXX-YYYY-ZZZZ", ExpirationDate = "2026-06-30" });
        SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "JetBrains All Products Pack", LicenseKey = "JB-APP-8829-1102", ExpirationDate = "2025-03-15" });
        SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "AWS Enterprise Support", LicenseKey = "AWS-902-114-882", ExpirationDate = "2024-12-31" });
        SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "Jira Cloud Premium", LicenseKey = "ATLAS-JIRA-99218", ExpirationDate = "2025-09-01" });
        SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "CrowdStrike Falcon", LicenseKey = "CS-SEC-0029-9912", ExpirationDate = "2026-01-15" });
        SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "Adobe Creative Cloud", LicenseKey = "ADOBE-CC-5521-8890", ExpirationDate = "2024-11-20" });
    }
}