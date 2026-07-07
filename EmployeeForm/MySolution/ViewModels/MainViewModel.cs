using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmployeeForm.Models;
using EmployeeForm.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;

namespace EmployeeForm.ViewModels;

public partial class MainViewModel : ObservableValidator
{
    public ObservableCollection<Employee> Employees { get; } = new();

    // ICollectionView is the standard WPF way to handle sorting, filtering, and grouping
    public ICollectionView EmployeesView { get; }

    public MainViewModel()
    {
        EmployeesView = CollectionViewSource.GetDefaultView(Employees);
        EmployeesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Employee.AgeGroup)));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be 2-50 characters.")]
    [NoNumbersAllowed]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Range(18, 65, ErrorMessage = "Age must be between 18 and 65.")]
    private int _age = 18;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private string _department = "IT";

    public string Summary => $"Adding: {Name} ({Department})";

    // Checks if there are any validation errors or empty required fields
    private bool CanSubmit => !HasErrors && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        // Double-check validation manually before committing
        ValidateAllProperties();
        if (HasErrors) return;

        Employees.Add(new Employee
        {
            Name = Name,
            Email = Email,
            Age = Age,
            Department = Department
        });

        // Reset form
        Name = string.Empty;
        Email = string.Empty;
        Age = 18;
        ClearErrors();
    }
}
