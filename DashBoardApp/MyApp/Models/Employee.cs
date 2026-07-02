using System;
using System.Collections.Generic;
using System.Text;

namespace DashBoardApp.Models;

public class Employee
{
    public int RoleId { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }

}

public class Role
{
    int Id { get; set; }
    string Name { get; set; }
}

public class DepartmentNode
{
    string Name { get; set; }
    List<DepartmentNode> Children { get; set; }
}
