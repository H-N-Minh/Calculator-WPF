using PriorityTaskBoard.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;


namespace PriorityTaskBoard.Models
{
    public partial class TaskItem : ObservableObject
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private TaskPriority _priority;
    }
}
