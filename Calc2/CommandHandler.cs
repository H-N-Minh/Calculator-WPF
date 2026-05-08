using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace Calc2;

public enum Operator
{
    Plus, Minus, Multiply, Divide
}

public partial class Calculator
{
    private void EnterEqual()
    {
        Debug.Assert(CurrentState != State.Operator, "Equal cant be entered after an operator");
        Debug.Assert(CurrentState != State.Start, "Equal cant be entered at the start");
        Debug.Assert(!(CurrentOperator == Operator.Divide && Buffer == 0), "Equal cant be entered for division by 0");

        // If only 1 number entered, pressing equal repetively should not change result.
        bool isOnlyBufferHasNumber = result == 0 && CurrentOperator == Operator.Plus && (CurrentState == State.Number || CurrentState == State.Float);

        // Updating fields and properties
        result = CalculateResult();
        Exponent = 0;
        if (isOnlyBufferHasNumber) Buffer = 0;
        CurrentState = State.Equal;
    }

    private void EnterOperator(string arithOperator)
    {
        Debug.Assert(CurrentState != State.Start, "Operator cant be entered at the start");

        // If operator is first entered after a number, calculate the result of the previous operation
        if (CurrentState == State.Number || CurrentState == State.Float)
        {
            result = CalculateResult();
        }

        Buffer = 0;
        Exponent = 0;
        CurrentOperator = arithOperator switch
        {
            "+" => Operator.Plus,
            "-" => Operator.Minus,
            "*" => Operator.Multiply,
            "/" => Operator.Divide,
            _ => throw new ArgumentException("arithOperator is not an operator", nameof(arithOperator))
        };
        CurrentState = State.Operator;
    }

    private void EnterClearAll()
    {
        result = 0;
        Buffer = 0;
        Exponent = 0;
        CurrentOperator = Operator.Plus;
        CurrentState = State.Start;
    }
    //private void EnterBackSpace(object? parameter)
    //{
    //    string number = Display;
    //    // delete last digit
    //    if (!string.IsNullOrEmpty(number))
    //    {
    //        number = number[..^1];
    //    }

    //    // if theres no digit left
    //    if (string.IsNullOrEmpty(number))
    //    {
    //        Buffer = 0;
    //        Exponent = 0;
    //        isTyping = false;
    //        return;
    //    }

    //    // else theres still digit left
    //    if (Exponent < 0) Exponent++;
    //    if (Exponent == -1)
    //    {
    //        number = number[..^1];
    //    }

    //    Buffer = decimal.Parse(number);
    //}
    private void EnterComma()
    {
        Debug.Assert(CurrentState != State.Float, "Comma cant be added to a float");
        Exponent--;
        CurrentState = State.Float;
    }
    private void EnterDigit(decimal digit)
    {
        Buffer = Exponent switch
        {
            0 => Buffer * 10 + digit,
            _ => Buffer + digit * (decimal)Math.Pow(10, Exponent--)
        };

        if (CurrentState != State.Float)
        {
            CurrentState = State.Number;
        }
        else
        {
            CurrentState = State.Float;
        }
    }

    private decimal CalculateResult()
        /* Update the Result by doing the calculation with the buffer using the current operator */
    {
        return CurrentOperator switch
        {
            Operator.Plus => result + buffer,
            Operator.Minus => result - buffer,
            Operator.Multiply => result * buffer,
            Operator.Divide => result / buffer,
            _ => throw new InvalidOperationException("CurrentOperator is not a valid math operator")
        };
    }
}
