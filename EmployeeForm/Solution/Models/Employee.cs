using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeForm.Models;

public class Employee
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Department { get; init; } = string.Empty;

    // Helper property used for Grouping in the View
    public string AgeGroup => Age <= 30 ? "18-30" : Age <= 50 ? "31-50" : "51+";
}