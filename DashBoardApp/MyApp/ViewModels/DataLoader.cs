using DashBoardApp.Models;
using DashBoardApp.ViewModels.ModelsVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.ViewModels;

public static class DataBase
{
    public static List<Role> FetchRawRolesFromDb()
    {
        return new List<Role>
        {
            new() { Id = 1, Name = "Director / VP" },
            new() { Id = 2, Name = "Team Lead" },
            new() { Id = 3, Name = "Senior Engineer" },
            new() { Id = 4, Name = "Engineer" },
            new() { Id = 5, Name = "Security Analyst" },
            new() { Id = 6, Name = "Support Specialist" },
            new() { Id = 7, Name = "UI/UX Designer" },
            new() { Id = 8, Name = "Data Scientist" },
            new() { Id = 9, Name = "Enterprise Architect" },
            new() { Id = 10, Name = "DevOps Intern" }
        };
    }

    public static List<DepartmentNode> FetchRawDepartmentsFromDb()
    {
        // Branch 1: IT
        var devDept = new DepartmentNode { Id = 2, Name = "Software Engineering" };
        devDept.Children.AddRange(new[] {
            new DepartmentNode { Id = 3, Name = "Core Backend Team" },
            new DepartmentNode { Id = 4, Name = "Frontend & Web Team" },
            new DepartmentNode { Id = 5, Name = "DevOps & Cloud Engineering" }
        });

        var opsDept = new DepartmentNode { Id = 6, Name = "IT Operations" };
        opsDept.Children.AddRange(new[] {
            new DepartmentNode { Id = 7, Name = "Infrastructure & Hardware" },
            new DepartmentNode { Id = 8, Name = "Cyber Security Operations" }
        });

        var supportDept = new DepartmentNode { Id = 9, Name = "Support Services" };
        supportDept.Children.AddRange(new[] {
            new DepartmentNode { Id = 10, Name = "Helpdesk Tier 1" },
            new DepartmentNode { Id = 11, Name = "Escalations Tier 2" }
        });

        var corporateIT = new DepartmentNode { Id = 1, Name = "Corporate IT Division" };
        corporateIT.Children.Add(devDept);
        corporateIT.Children.Add(opsDept);
        corporateIT.Children.Add(supportDept);

        // Branch 2: Marketing
        var creativeDept = new DepartmentNode { Id = 13, Name = "Creative & Design" };
        creativeDept.Children.AddRange(new[] {
            new DepartmentNode { Id = 14, Name = "UI/UX Design Studio" },
            new DepartmentNode { Id = 15, Name = "Multimedia & Video Production" }
        });

        var analyticsDept = new DepartmentNode { Id = 16, Name = "Data & Analytics" };
        analyticsDept.Children.AddRange(new[] {
            new DepartmentNode { Id = 17, Name = "SEO & Growth Hacking" },
            new DepartmentNode { Id = 18, Name = "Business Intelligence" }
        });

        var marketingOps = new DepartmentNode { Id = 19, Name = "Marketing Operations" };
        marketingOps.Children.Add(new DepartmentNode { Id = 20, Name = "Social Media Strategy" });
        marketingOps.Children.Add(new DepartmentNode { Id = 21, Name = "Paid Advertising Campaigns" });

        var digitalMarketing = new DepartmentNode { Id = 12, Name = "Digital Marketing Division" };
        digitalMarketing.Children.Add(creativeDept);
        digitalMarketing.Children.Add(analyticsDept);
        digitalMarketing.Children.Add(marketingOps);

        return new List<DepartmentNode> { corporateIT, digitalMarketing };
    }

    public static List<Employee> FetchRawEmployeesFromDb()
    {
        return new List<Employee>
        {
            new() { Name = "Sarah Jenkins", RoleId = 1, DepartmentId = 1 },
            new() { Name = "David Chen", RoleId = 2, DepartmentId = 2 },
            new() { Name = "Elena Rostova", RoleId = 3, DepartmentId = 3 },
            new() { Name = "Marcus Aurelius", RoleId = 4, DepartmentId = 3 },
            new() { Name = "Linus Torvalds", RoleId = 3, DepartmentId = 5 },
            new() { Name = "Aisha Rahman", RoleId = 2, DepartmentId = 6 },
            new() { Name = "John Doe", RoleId = 4, DepartmentId = 7 },
            new() { Name = "Karen Smith", RoleId = 5, DepartmentId = 8 },
            new() { Name = "Liam O'Connor", RoleId = 5, DepartmentId = 8 },
            new() { Name = "Sanjay Patel", RoleId = 6, DepartmentId = 10 },
            new() { Name = "Chloe Dupont", RoleId = 6, DepartmentId = 10 },
            new() { Name = "Yuki Tanaka", RoleId = 6, DepartmentId = 11 },
            new() { Name = "Oliver Hansen", RoleId = 1, DepartmentId = 12 },
            new() { Name = "Emma Watson", RoleId = 7, DepartmentId = 14 },
            new() { Name = "Lucas Vance", RoleId = 8, DepartmentId = 18 },
            new() { Name = "Sophia Martinez", RoleId = 2, DepartmentId = 19 },
            new() { Name = "Ryan Reynolds", RoleId = 4, DepartmentId = 20 }
        };
    }
}

public static class DataLoader
{
    public static void LoadDummyData(InventoryVM inventoryViewModel, EmployeeTabVM employeeViewModel)
    {
        // ==========================================
        // PHASE 1: Simulate Raw Database Fetch (Models Only)
        // ==========================================
        List<Role> rawRoles = DataBase.FetchRawRolesFromDb();
        List<DepartmentNode> rawDepartments = DataBase.FetchRawDepartmentsFromDb();
        List<Employee> rawEmployees = DataBase.FetchRawEmployeesFromDb();

        // ==========================================
        // PHASE 2: Convert Models to ViewModels (WPF Layer)
        // ==========================================

        // 1. Clear static caches and collections
        RoleVM.AllRoles.Clear();
        DepartmentNodeVM.AllDepartments.Clear();
        employeeViewModel.AllEmployees.Clear();
        employeeViewModel.AllDepartments.Clear();
        employeeViewModel.AllRoles.Clear();

        // 2. Instantiate Role ViewModels
        foreach (var roleModel in rawRoles)
        {
            employeeViewModel.AllRoles.Add(new RoleVM(roleModel));
        }

        // 3. Instantiate Department ViewModels (Recursively registers all children)
        foreach (var deptModel in rawDepartments)
        {
            employeeViewModel.AllDepartments.Add(new DepartmentNodeVM(deptModel));
        }

        // 4. Instantiate Employee ViewModels (Safely links to resolved Role/Dept VMs via dictionaries)
        foreach (var empModel in rawEmployees)
        {
            employeeViewModel.AllEmployees.Add(new EmployeeVM(empModel));
        }

        // 5. Load Hardware Assets
        LoadHardwares(inventoryViewModel, employeeViewModel.AllEmployees);
    }


    private static void LoadHardwares(InventoryVM inventoryViewModel, ObservableCollection<EmployeeVM> employees)
    {
        ObservableCollection<HardwareAsset> hardwares = inventoryViewModel.Hardwares;
        hardwares.Clear();

        // Safe index checking before mapping mock equipment to our new EmployeeVM items
        if (employees.Count > 13)
        {
            hardwares.Add(new HardwareAsset { AssetId = "LT-9921", Type = "MacBook Pro M3 Max", Manufacturer = "Apple", AssignedTo = employees[0] });
            hardwares.Add(new HardwareAsset { AssetId = "LT-4412", Type = "ThinkPad P1 Gen 6", Manufacturer = "Lenovo", AssignedTo = employees[1] });
            hardwares.Add(new HardwareAsset { AssetId = "SRV-8801", Type = "PowerEdge R760 Rack Server", Manufacturer = "Dell", AssignedTo = employees[2] });
            hardwares.Add(new HardwareAsset { AssetId = "SW-2209", Type = "Catalyst 9300 48-Port Switch", Manufacturer = "Cisco", AssignedTo = employees[3] });
            hardwares.Add(new HardwareAsset { AssetId = "LT-8840", Type = "ThinkPad X1 Carbon Gen 11", Manufacturer = "Lenovo", AssignedTo = employees[4] });
            hardwares.Add(new HardwareAsset { AssetId = "LT-1029", Type = "Latitude 5440 VPro", Manufacturer = "Dell", AssignedTo = employees[5] });
            hardwares.Add(new HardwareAsset { AssetId = "MN-5541", Type = "UltraSharp 34\" Curved Monitor", Manufacturer = "Dell", AssignedTo = employees[6] });
            hardwares.Add(new HardwareAsset { AssetId = "FW-4001", Type = "Firepower 2110 Appliance", Manufacturer = "Cisco", AssignedTo = employees[7] });
            hardwares.Add(new HardwareAsset { AssetId = "PH-3310", Type = "IP Phone 8841 Multiplatform", Manufacturer = "Cisco", AssignedTo = employees[9] });
            hardwares.Add(new HardwareAsset { AssetId = "LT-5520", Type = "MacBook Air M3", Manufacturer = "Apple", AssignedTo = employees[12] });
            hardwares.Add(new HardwareAsset { AssetId = "TAB-091", Type = "iPad Pro 12.9\" M2", Manufacturer = "Apple", AssignedTo = employees[13] });
        }
    }
}