using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Calc2;

public enum State
{
    Start, Equal, Number, Float, Operator
}

public partial class Calculator : INotifyPropertyChanged
{
    // Fields
    private decimal result = 0;

    // Properties
    private State state = State.Start;
    public State State
    {
        get { return state; }
        set { state = value; }
    }


    private Operator currentOperator = Operator.Plus;
    public Operator CurrentOperator
    {
        get { return currentOperator; }
        set
        {
            currentOperator = value;
            OnPropertyChanged(nameof(CurrentOperator));
        }
    }

    private int exponent;
    public int Exponent
    {
        get { return exponent; }
        set
        {
            exponent = value;
            OnPropertyChanged(nameof(Exponent));
            OnPropertyChanged(nameof(Display));
        }
    }

    private decimal buffer;       
    public decimal Buffer
    {
        get { return buffer; }
        set
        {                       // Notify changes to buffer (also Display)
            buffer = value;
            OnPropertyChanged(nameof(Buffer));
            OnPropertyChanged(nameof(Display));
        }
    }

    public string Display       // Readonly, string version of buffer
    {
        get
        {
            return Exponent switch
            {
                -1 => buffer.ToString() + ",",
                _ => buffer.ToString()
            };
        }
    }

    public CommandHandler CommandHandler { get; set; }  

    // Event
    public event PropertyChangedEventHandler? PropertyChanged;


    /* ########################################################## */
    // Ctor
    public Calculator()
    {
        CommandHandler = new CommandHandler();
    }

    // Method
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}

public class CommandHandler : ICommand
{
    // Fields
    private Calculator calculator;

    // Ctor
    public CommandHandler(Calculator calc)
    {
        calculator = calc;
    }

    // Event
    public event EventHandler? CanExecuteChanged;

    // ICommand implementation
    public bool CanExecute(object? parameter)
    {
        string command = parameter as string ?? throw new ArgumentException("Parameter must be a string", nameof(parameter));

        switch (command)
        {
            case "C": case "+/-": case "0": case "1": case "2": case "3": case "4": case "5": case "6": case "7": case "8": case "9":
                break;
            case "BackSpace": case "CE": case "=":
                if (calculator.State == State.Operator) return false;
                break;
            case "*": case "/": case "-": case "+":
                if (calculator.State == State.Start) return false;
                break;
            case ",":
                if (calculator.State == State.Float) return false;
                break;
            default:
                throw new ArgumentException("Not implemented command", nameof(parameter));
        }

        return true;
    }

    public void Execute(object? parameter)
    {
        throw new NotImplementedException();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}