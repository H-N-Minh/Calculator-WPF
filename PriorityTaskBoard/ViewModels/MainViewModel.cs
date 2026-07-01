using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriorityTaskBoard.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace PriorityTaskBoard.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUserTyping))]
        [NotifyCanExecuteChangedFor(nameof(AddTaskCommand))]
        private string _newTaskTitle = string.Empty;

        public bool IsUserTyping => NewTaskTitle != string.Empty;

        [ObservableProperty]
        private TaskPriority _selectedPriority = TaskPriority.Low;

        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public ObservableCollection<TaskPriority> AvailablePriorities { get; } =
            new ObservableCollection<TaskPriority>(Enum.GetValues<TaskPriority>());

        [RelayCommand(CanExecute = nameof(CanAddTask))]
        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            Tasks.Add(new TaskItem
            {
                Title = NewTaskTitle,
                Priority = SelectedPriority,
                IsCompleted = false
            });

            NewTaskTitle = string.Empty;
        }

        private bool CanAddTask() => NewTaskTitle != string.Empty;
    }

}
