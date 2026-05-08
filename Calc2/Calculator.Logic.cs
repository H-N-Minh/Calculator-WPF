using System;
using System.Collections.Generic;
using System.Text;

namespace Calc2;

public enum Operator
{
    Plus, Minus, Divide, Multiply
}

public partial class Calculator
{
    private void enterEqual(object? parameter)
    {
        Buffer = currentOperator switch
        {
            Operator.Plus => result + buffer,
            Operator.Minus => result - buffer,
            Operator.Multiply => result * buffer,
            Operator.Divide => result / buffer,
            _ => result
        };
        result = 0;
        exponent = 0;
        CurrentOperator = Operator.Plus;
        isTyping = true;
    }
    private bool isStartState(object? parameter)
    {
        return result == 0 && buffer == 0;
    }
    private void enterOperator(object? parameter)
    {
        string param = parameter?.ToString() ?? "+";
        if (IsTyping)
        {
            result = currentOperator switch
            {
                Operator.Plus => result + buffer,
                Operator.Minus => result - buffer,
                Operator.Multiply => result * buffer,
                Operator.Divide => result * buffer,
                _ => result
            };
            buffer = 0;
            exponent = 0;
        }

        currentOperator = param switch
        {
            "+" => Operator.Plus,
            "-" => Operator.Minus,
            "*" => Operator.Multiply,
            "/" => Operator.Divide,
            _ => Operator.Plus
        };

        IsTyping = false;
    }
    private void enterClearAll(object? parameter)
    {
        result = 0;
        Buffer = 0;
        Exponent = 0;
        CurrentOperator = Operator.Plus;
        IsTyping = false;
    }
    private void enterBackSpace(object? parameter)
    {
        string number = Display;
        // delete last digit
        if (!string.IsNullOrEmpty(number))
        {
            number = number[..^1];
        }

        // if theres no digit left
        if (string.IsNullOrEmpty(number))
        {
            Buffer = 0;
            Exponent = 0;
            isTyping = false;
            return;
        }

        // else theres still digit left
        if (Exponent < 0) Exponent++;
        if (Exponent == -1)
        {
            number = number[..^1];
        }

        Buffer = decimal.Parse(number);
    }
    private void enterComma(object? parameter)
    {
        Exponent--;
        IsTyping = true;
    }
    private void enterDigit(object? parameter)
    {
        Buffer = Exponent switch
        {
            0 => Buffer * 10 + decimal.Parse(parameter?.ToString() ?? "0"),
            _ => Buffer + decimal.Parse(parameter?.ToString() ?? "0") * (decimal)Math.Pow(10, Exponent--)
        };
        IsTyping = true;
    }

}
