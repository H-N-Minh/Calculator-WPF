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
        

        // Trigger filter refresh
        SelectedRoleId = 0;
        OnSelectedRoleIdChanged(0);

        //// 5. Setup DataGrid Data (Expanded Software Licensing)
        //SoftwareLicenses.Clear();
        //SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "Microsoft 365 Enterprise", LicenseKey = "E5-M365-XXXX-YYYY-ZZZZ", ExpirationDate = "2026-06-30" });
        //SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "JetBrains All Products Pack", LicenseKey = "JB-APP-8829-1102", ExpirationDate = "2025-03-15" });
        //SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "AWS Enterprise Support", LicenseKey = "AWS-902-114-882", ExpirationDate = "2024-12-31" });
        //SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "Jira Cloud Premium", LicenseKey = "ATLAS-JIRA-99218", ExpirationDate = "2025-09-01" });
        //SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "CrowdStrike Falcon", LicenseKey = "CS-SEC-0029-9912", ExpirationDate = "2026-01-15" });
        //SoftwareLicenses.Add(new SoftwareLicense { SoftwareName = "Adobe Creative Cloud", LicenseKey = "ADOBE-CC-5521-8890", ExpirationDate = "2024-11-20" });
    }
}