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
    private State currentState = State.Start;
    public State CurrentState
    {
        get { return currentState; }
        set { 
            currentState = value;
            OnPropertyChanged(nameof(CurrentState));
            Handler.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Display));
        }
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

    public string Display       // Readonly, string version of buffer or result
    {
        get
        {
            if (CurrentState == State.Equal)
            {
                return result.ToString();
            }
            return Exponent switch
            {
                -1 => buffer.ToString() + ",",
                _ => buffer.ToString()
            };
        }
    }

    public CommandHandler Handler { get; set; }  

    // Event
    public event PropertyChangedEventHandler? PropertyChanged;


    /* ########################################################## */
    // Ctor
    public Calculator()
    {
        Handler = new CommandHandler(this);
    }

    // Method
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /* ########################################################## */
    // Nested Class
    public class CommandHandler : ICommand
    {
        // Fields
        private Calculator calculator;

        // Ctor
        public CommandHandler(Calculator calc)
        {
            calculator = calc;
        }

        // ICommand implementation
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            string command = parameter as string ?? throw new ArgumentException("Parameter must be a string", nameof(parameter));

            switch (command)
            {
                case "C": case "0": case "1": case "2": case "3": case "4": case "5": case "6": case "7": case "8": case "9":
                    break;
                case "+/-":
                    if (calculator.CurrentState == State.Operator || calculator.CurrentState == State.Start) return false;
                    break;
                case "=":
                    if (calculator.CurrentState == State.Operator || calculator.CurrentState == State.Start) return false;
                    // Check for division by zero
                    if (calculator.CurrentOperator == Operator.Divide && calculator.Buffer == 0)
                    {
                        return false;
                    }
                    break;
                case "BackSpace":
                    if (calculator.CurrentState == State.Operator || calculator.CurrentState == State.Start || calculator.CurrentState == State.Equal ||
                        calculator.Buffer == 0) return false;
                    break;
                case "CE":
                    if (calculator.CurrentState == State.Operator) return false;
                    break;
                case "*": case "/": case "-": case "+":
                    if (calculator.CurrentState == State.Start) return false;
                    break;
                case ",":
                    if (calculator.CurrentState == State.Float || calculator.CurrentState == State.Operator || calculator.CurrentState == State.Equal) return false;
                    break;
                default:
                    throw new ArgumentException("Not implemented command", nameof(parameter));
            }

            return true;
        }

        public void Execute(object? parameter)
        {
            string command = parameter as string ?? throw new ArgumentException("Parameter must be a string", nameof(parameter));

            switch (command)
            {
                case "0": case "1": case "2": case "3": case "4": case "5": case "6": case "7": case "8": case "9":
                    calculator.EnterDigit(int.Parse(command)); 
                    break;
                case "C":
                    calculator.EnterClearAll();
                    break;
                case "=":
                    calculator.EnterEqual();
                    break;
                case "BackSpace":
                    calculator.EnterBackSpace();
                    break;
                case "+/-":
                    calculator.EnterSignFlip();
                    break;
                case "CE":
                    calculator.EnterClearEntry();
                    break;
                case "*": case "/": case "-": case "+":
                    calculator.EnterOperator(command);
                    break;
                case ",":
                    calculator.EnterComma();
                    break;
                default:
                    throw new ArgumentException("Not implemented command", nameof(parameter));
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}