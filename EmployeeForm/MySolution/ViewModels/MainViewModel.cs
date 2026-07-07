
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySolution.ViewModels.ModelsVM;
using System.Collections.ObjectModel;

namespace MySolution.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<EmployeeVM> Employees { get; set; } = new();

    [ObservableProperty]
    private EmployeeVM currentEmployee;

    private void AddEmployee(EmployeeVM employeeVM)
    {
        Employees.Add(employeeVM);
        CurrentEmployee = new EmployeeVM(new Models.Employee(), AddEmployee);
    }

    public MainViewModel()
    {
        CurrentEmployee = new EmployeeVM(new Models.Employee(), AddEmployee);
    }
}
