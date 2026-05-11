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

public partial class Calculator : INotifyPropertyChanged
{
    // Fields
    private decimal result = 0;

    private OperatorStrategy operatorState;
    private EqualStrategy equalState;
    private FloatStrategy floatState;
    private StartStrategy startState;
    private CalculatorStrategy numberState; 

    // Properties
    private ICalculatorStrategy currentState;
    public ICalculatorStrategy CurrentState
    {
        get { return currentState; }
        set
        {
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
            if (CurrentState == equalState)
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
        numberState = new CalculatorStrategy(this);
        startState = new StartStrategy(numberState);
        operatorState = new OperatorStrategy(numberState);
        equalState = new EqualStrategy(numberState);
        floatState = new FloatStrategy(numberState);

        CurrentState = startState;
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

            return calculator.CurrentState.CanExecute(command);
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


public interface ICalculatorStrategy
{
    bool CanExecute(string command);
}

public class CalculatorStrategy (Calculator calculator) : ICalculatorStrategy
{
    public bool CanExecute(string command)
    {
        // No backspace if buffer is empty
        if (command == "BackSpace" && calculator.Display == "0")
        {
            return false;
        }

        if (calculator.CurrentOperator == Operator.Divide && calculator.Buffer == 0 && command == "=")
        {
            return false;
        }
        return true;
    }

}


public class  FloatStrategy (ICalculatorStrategy baseState, CanEval canEval)  : ICalculatorStrategy
{
    public bool CanExecute(string command)
    {
        return command != "," && baseState.CanExecute(command);
    }

    public void Execute(string command)
    {
        if (command == "=")
        {
            canEval.Execute();
        }
    }

}

public class EqualStrategy(ICalculatorStrategy baseState) : ICalculatorStrategy
{
    public bool CanExecute(string command)
    {
        bool isValidCommand = command != "BackSpace" && command != ",";
        return isValidCommand && baseState.CanExecute(command);
    }

    public void Execute(string command)
    {
        if (command == "=")
        {

        }
    }
}



public class StartStrategy(ICalculatorStrategy baseState) : ICalculatorStrategy
{
    public bool CanExecute(string command)
    {
        return command is not ("*" or "/" or "-" or "+" or "BackSpace" or "=" or "+/-") && baseState.CanExecute(command);
    }
}

public class OperatorStrategy(ICalculatorStrategy baseState) : ICalculatorStrategy
{
    public bool CanExecute(string command)
    {
        return command is not ("CE" or "," or "+/-" or "=" or "BackSpace") && baseState.CanExecute(command);
    }

    public void Execute(string command)
    {

        //switch (command)
        //{
        //    case "*": case "/": case "-": case "+":
        //        calculator.EnterOperator(command);
        //        break;
        //    default:
        //        throw new ArgumentException("Not implemented command", nameof(command));
        //}
    }
}


public class CanEval(Calculator calculator)
{
    public void Execute()
    {
        
    }


}