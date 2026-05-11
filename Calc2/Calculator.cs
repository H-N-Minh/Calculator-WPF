using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
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

    private CalculatorState calculatorState;
    private OperatorState operatorState = new OperatorState();
    private EqualState equalState = new EqualState();
    private FloatState floatState = new FloatState();
    private StartState startState = new StartState();
    private CalculatorState numberState = new CalculatorState();
    public CalculatorState CalculatorState
    {
        get { return calculatorState; }
        set
        {
            calculatorState = value;
            OnPropertyChanged(nameof(CalculatorState));
            Handler.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(Display));
        }
    }


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
        CalculatorState = startState;

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

            return calculator.CalculatorState.CanExecute(command, calculator);
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


public class CalculatorState
{
    public virtual bool CanExecute(string command, Calculator calculator)
    {
        // Check for division by zero
        if (calculator.CurrentOperator == Operator.Divide && calculator.Buffer == 0)
        {
            return false;
        }
        if (command == "BackSpace" && calculator.Buffer == 0)
        {
            return false;
        }
        return true;
    }

}

public class  FloatState : CalculatorState
{
    public override bool CanExecute(string command, Calculator calculator)
    {
        switch (command)
        {
            case ",":
                return false;
        }
        return base.CanExecute(command, calculator);
    }
}

public class EqualState : CalculatorState
{
    public override bool CanExecute(string command, Calculator calculator)
    {
        switch (command)
        {
            case "BackSpace":
            case ",":
                return false;
        }
        return base.CanExecute(command, calculator);
    }
}

public class StartState : CalculatorState
{
    public override bool CanExecute(string command, Calculator calculator)
    {
        switch (command)
        {
            case "*":
            case "/":
            case "-":
            case "+":
            case "BackSpace":
            case "=":
            case "+/-":
                return false;
        }
        return base.CanExecute(command, calculator);
    }
}

public class OperatorState : CalculatorState
{
    public override bool CanExecute(string command, Calculator calculator)
    {
        switch (command)
        {
            case "CE":
            case ",":
            case "+/-":
            case "=":
                return false;
        }
        return base.CanExecute(command, calculator);
    }
}