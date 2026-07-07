using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySolution.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MySolution.ViewModels.ModelsVM;

public partial class EmployeeVM : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage ="Name is too long")]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private string name = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage ="Not a valid email")]
    private string email = string.Empty;

    [ObservableProperty]
    [Range(18, 65, ErrorMessage ="Age must be between 18 and 65")]
    private int age;

    [ObservableProperty]
    private string department = string.Empty;

    public ObservableCollection<string> AllDepartments { get; set; } = 
        new ObservableCollection<string>() { "IT", "Sales", "HR" };

    public string Summary => $"Adding employee {Name}";


    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        ValidateAllProperties();

        if (HasErrors) return;

        AddEmployee(this);
    }
    private bool CanSubmit() => !HasErrors;

    private Action<EmployeeVM> AddEmployee;

    public EmployeeVM(Employee employee, Action<EmployeeVM> addEmployee)
    {
        Name = employee.Name;
        Email = employee.Email;
        Age = employee.Age;
        Department = employee.Department;
        AddEmployee = addEmployee;

        ErrorsChanged += (s, e) => SubmitCommand.NotifyCanExecuteChanged();
    }
}
