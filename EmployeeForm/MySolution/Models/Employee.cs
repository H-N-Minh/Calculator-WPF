using System;
using System.Collections.Generic;
using System.Text;

namespace MySolution.Models;

public class Employee
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Department { get; set; } = string.Empty;
}