using DashBoardApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DashBoardApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for EmployeeTab.xaml
    /// </summary>
    public partial class EmployeeTab : UserControl
    {
        public static readonly DependencyProperty DepartmentsProperty =
            DependencyProperty.Register(
                nameof(Departments),
                typeof(ObservableCollection<DepartmentNode>),
                typeof(EmployeeTab),
                new PropertyMetadata(null));

        public ObservableCollection<DepartmentNode> Departments
        {
            get => (ObservableCollection<DepartmentNode>)GetValue(DepartmentsProperty);
            set => SetValue(DepartmentsProperty, value);
        }

        public static readonly DependencyProperty SelectedDepartmentProperty =
            DependencyProperty.Register(
                nameof(SelectedDepartment),
                typeof(DepartmentNode),
                typeof(EmployeeTab),
                new PropertyMetadata(null));

        public DepartmentNode SelectedDepartment
        {
            get => (DepartmentNode)GetValue(DepartmentsProperty);
            set => SetValue(DepartmentsProperty, value);
        }

        public EmployeeTab()
        {
            InitializeComponent();
        }
    }
}
