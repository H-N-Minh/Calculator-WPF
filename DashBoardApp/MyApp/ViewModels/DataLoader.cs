using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashBoardApp.ViewModels;

public static class DataLoader
{
    public static void LoadDummyData(InventoryVM inventoryViewModel, EmployeeVM employeeViewModel)
    {
        // 1. Load lookups and hierarchies first
        LoadRoles(employeeViewModel);
        LoadDepartments(employeeViewModel);

        // 2. Load employees who depend on those departments and roles
        LoadEmployees(employeeViewModel);

        // 3. Load hardware assets assigned to the generated employees
        LoadHardwares(inventoryViewModel, employeeViewModel.AllEmployees);
    }

    private static void LoadRoles(EmployeeVM employeeVM)
    {
        ObservableCollection<Role> AllRoles = employeeVM.AllRoles;
        AllRoles.Clear();

        // Standard Lookup Roles
        AllRoles.Add(new Role { Id = 0, Name = "All Roles" });
        AllRoles.Add(new Role { Id = 1, Name = "Director / VP" });
        AllRoles.Add(new Role { Id = 2, Name = "Team Lead" });
        AllRoles.Add(new Role { Id = 3, Name = "Senior Engineer" });
        AllRoles.Add(new Role { Id = 4, Name = "Engineer" });
        AllRoles.Add(new Role { Id = 5, Name = "Security Analyst" });
        AllRoles.Add(new Role { Id = 6, Name = "Support Specialist" });
        AllRoles.Add(new Role { Id = 7, Name = "UI/UX Designer" });
        AllRoles.Add(new Role { Id = 8, Name = "Data Scientist" });

        // COMPLEXITY REQUIREMENT: Roles defined in the system but currently UNFILLED by any employee
        AllRoles.Add(new Role { Id = 9, Name = "Enterprise Architect" });
        AllRoles.Add(new Role { Id = 10, Name = "DevOps Intern" });
    }

    private static void LoadDepartments(EmployeeVM employeeVM)
    {
        employeeVM.AllDepartments.Clear();

        // --- BRANCH 1: CORPORATE IT DIVISION ---
        var devDept = new DepartmentNode { Id = 2, Name = "Software Engineering" };
        devDept.Children.Add(new DepartmentNode { Id = 3, Name = "Core Backend Team" });
        devDept.Children.Add(new DepartmentNode { Id = 4, Name = "Frontend & Web Team" });
        devDept.Children.Add(new DepartmentNode { Id = 5, Name = "DevOps & Cloud Engineering" });

        var opsDept = new DepartmentNode { Id = 6, Name = "IT Operations" };
        opsDept.Children.Add(new DepartmentNode { Id = 7, Name = "Infrastructure & Hardware" });
        opsDept.Children.Add(new DepartmentNode { Id = 8, Name = "Cyber Security Operations" });

        var supportDept = new DepartmentNode { Id = 9, Name = "Support Services" };
        supportDept.Children.Add(new DepartmentNode { Id = 10, Name = "Helpdesk Tier 1" });
        supportDept.Children.Add(new DepartmentNode { Id = 11, Name = "Escalations Tier 2" });

        var corporateIT = new DepartmentNode { Id = 1, Name = "Corporate IT Division" };
        corporateIT.Children.Add(devDept);
        corporateIT.Children.Add(opsDept);
        corporateIT.Children.Add(supportDept);

        employeeVM.AllDepartments.Add(corporateIT);

        // --- BRANCH 2: DIGITAL MARKETING DIVISION ---
        var creativeDept = new DepartmentNode { Id = 13, Name = "Creative & Design" };
        creativeDept.Children.Add(new DepartmentNode { Id = 14, Name = "UI/UX Design Studio" });
        creativeDept.Children.Add(new DepartmentNode { Id = 15, Name = "Multimedia & Video Production" });

        var analyticsDept = new DepartmentNode { Id = 16, Name = "Data & Analytics" };
        analyticsDept.Children.Add(new DepartmentNode { Id = 17, Name = "SEO & Growth Hacking" });
        analyticsDept.Children.Add(new DepartmentNode { Id = 18, Name = "Business Intelligence" });

        var marketingOps = new DepartmentNode { Id = 19, Name = "Marketing Operations" };
        marketingOps.Children.Add(new DepartmentNode { Id = 20, Name = "Social Media Strategy" });

        // COMPLEXITY REQUIREMENT: A department node that exists but is completely empty/unstaffed
        marketingOps.Children.Add(new DepartmentNode { Id = 21, Name = "Paid Advertising Campaigns" });

        var digitalMarketing = new DepartmentNode { Id = 12, Name = "Digital Marketing Division" };
        digitalMarketing.Children.Add(creativeDept);
        digitalMarketing.Children.Add(analyticsDept);
        digitalMarketing.Children.Add(marketingOps);

        employeeVM.AllDepartments.Add(digitalMarketing);
    }

    private static void LoadEmployees(EmployeeVM employeeVM)
    {
        ObservableCollection<Employee> AllEmployees = employeeVM.AllEmployees;
        AllEmployees.Clear();

        // Corporate IT Leadership & Staff
        AllEmployees.Add(new Employee { Name = "Sarah Jenkins", RoleId = 1, DepartmentId = 1 });    // Director @ Corporate IT
        AllEmployees.Add(new Employee { Name = "David Chen", RoleId = 2, DepartmentId = 2 });       // Lead @ Software Engineering
        AllEmployees.Add(new Employee { Name = "Elena Rostova", RoleId = 3, DepartmentId = 3 });    // Senior @ Core Backend
        AllEmployees.Add(new Employee { Name = "Marcus Aurelius", RoleId = 4, DepartmentId = 3 });  // Engineer @ Core Backend
        AllEmployees.Add(new Employee { Name = "Linus Torvalds", RoleId = 3, DepartmentId = 5 });   // Senior @ DevOps

        // IT Operations & Security
        AllEmployees.Add(new Employee { Name = "Aisha Rahman", RoleId = 2, DepartmentId = 6 });     // Lead @ IT Operations
        AllEmployees.Add(new Employee { Name = "John Doe", RoleId = 4, DepartmentId = 7 });         // Engineer @ Infrastructure
        AllEmployees.Add(new Employee { Name = "Karen Smith", RoleId = 5, DepartmentId = 8 });      // Security Analyst @ Cyber Security
        AllEmployees.Add(new Employee { Name = "Liam O'Connor", RoleId = 5, DepartmentId = 8 });   // Security Analyst @ Cyber Security

        // Support Services
        AllEmployees.Add(new Employee { Name = "Sanjay Patel", RoleId = 6, DepartmentId = 10 });    // Support Specialist @ Helpdesk Tier 1
        AllEmployees.Add(new Employee { Name = "Chloe Dupont", RoleId = 6, DepartmentId = 10 });    // Support Specialist @ Helpdesk Tier 1
        AllEmployees.Add(new Employee { Name = "Yuki Tanaka", RoleId = 6, DepartmentId = 11 });     // Support Specialist @ Escalations Tier 2

        // Digital Marketing Division
        AllEmployees.Add(new Employee { Name = "Oliver Hansen", RoleId = 1, DepartmentId = 12 });   // Director @ Digital Marketing
        AllEmployees.Add(new Employee { Name = "Emma Watson", RoleId = 7, DepartmentId = 14 });     // UI/UX Designer @ UI/UX Design Studio
        AllEmployees.Add(new Employee { Name = "Lucas Vance", RoleId = 8, DepartmentId = 18 });     // Data Scientist @ Business Intelligence
        AllEmployees.Add(new Employee { Name = "Sophia Martinez", RoleId = 2, DepartmentId = 19 }); // Lead @ Marketing Operations
        AllEmployees.Add(new Employee { Name = "Ryan Reynolds", RoleId = 4, DepartmentId = 20 });   // Engineer/Strategist @ Social Media

        // Note: DepartmentId = 21 (Paid Advertising Campaigns) is purposefully omitted here. It has NO employees.
        // Note: RoleId = 9 (Enterprise Architect) and 10 (DevOps Intern) are purposefully omitted. They have NO assigned employees.
    }

    private static void LoadHardwares(InventoryVM inventoryViewModel, ObservableCollection<Employee> allEmployees)
    {
        ObservableCollection<HardwareAsset> Hardwares = inventoryViewModel.Hardwares;
        Hardwares.Clear();

        // Dynamically assign realistic deployment gear based on their roles
        Hardwares.Add(new HardwareAsset { AssetId = "LT-9921", Type = "MacBook Pro M3 Max", Manufacturer = "Apple", AssignedTo = allEmployees[0] });
        Hardwares.Add(new HardwareAsset { AssetId = "LT-4412", Type = "ThinkPad P1 Gen 6", Manufacturer = "Lenovo", AssignedTo = allEmployees[1] });
        Hardwares.Add(new HardwareAsset { AssetId = "SRV-8801", Type = "PowerEdge R760 Rack Server", Manufacturer = "Dell", AssignedTo = allEmployees[2] });
        Hardwares.Add(new HardwareAsset { AssetId = "SW-2209", Type = "Catalyst 9300 48-Port Switch", Manufacturer = "Cisco", AssignedTo = allEmployees[3] });
        Hardwares.Add(new HardwareAsset { AssetId = "LT-8840", Type = "ThinkPad X1 Carbon Gen 11", Manufacturer = "Lenovo", AssignedTo = allEmployees[4] });

        Hardwares.Add(new HardwareAsset { AssetId = "LT-1029", Type = "Latitude 5440 VPro", Manufacturer = "Dell", AssignedTo = allEmployees[5] });
        Hardwares.Add(new HardwareAsset { AssetId = "MN-5541", Type = "UltraSharp 34\" Curved Monitor", Manufacturer = "Dell", AssignedTo = allEmployees[6] });
        Hardwares.Add(new HardwareAsset { AssetId = "FW-4001", Type = "Firepower 2110 Appliance", Manufacturer = "Cisco", AssignedTo = allEmployees[7] });

        Hardwares.Add(new HardwareAsset { AssetId = "PH-3310", Type = "IP Phone 8841 Multiplatform", Manufacturer = "Cisco", AssignedTo = allEmployees[9] });
        Hardwares.Add(new HardwareAsset { AssetId = "LT-5520", Type = "MacBook Air M3", Manufacturer = "Apple", AssignedTo = allEmployees[12] });
        Hardwares.Add(new HardwareAsset { AssetId = "TAB-091", Type = "iPad Pro 12.9\" M2", Manufacturer = "Apple", AssignedTo = allEmployees[13] });
    }
}