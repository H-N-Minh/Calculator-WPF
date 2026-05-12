using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace YT;

public class RelayCommand(Action<object?> exe, Func<object?, bool> canExe = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return canExe?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        exe(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
