using System.Windows.Input;
using System.Windows.Navigation;

namespace Calc2;


public class RelayCommand(
    Action<object?> execute, 
    Func<object?, bool>? canExecute = null
    ) : ICommand
{

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
        => canExecute is null || canExecute(parameter); 

    public void Execute(object? parameter) 
        => execute(parameter);

    public void RaiseCanExecuteChanged() 
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
