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

    private void EnterComma()
    {
        Debug.Assert(CurrentState != State.Float, "Comma cant be added to a float");
        Debug.Assert(CurrentState != State.Operator, "Comma cant be entered after an operator");
        Debug.Assert(CurrentState != State.Equal, "Comma cant be entered after Equal button");

        Exponent--;
        CurrentState = State.Float;
    }

    private void EnterDigit(decimal digit)
    {
        if (CurrentState == State.Equal)    EnterClearAll();

        Buffer = Exponent switch
        {
            0 => Buffer * 10 + digit,
            _ => Buffer + digit * (decimal)Math.Pow(10, Exponent--)
        };

        if (CurrentState == State.Float)
        {
            CurrentState = State.Float;
        }
        else
        {
            CurrentState = State.Number;
        }
    }

    private void EnterBackSpace()
    {
        Debug.Assert(CurrentState != State.Start, "Backspace cant be entered at the start");
        Debug.Assert(CurrentState != State.Operator, "Backspace cant be entered after an operator");
        Debug.Assert(CurrentState != State.Equal, "Backspace cant be entered after Equal button");
        Debug.Assert(Buffer != 0, "Backspace cant be entered when theres no digit");

        string currentDisplay = Display;

        // If there is only 1 character left, reset
        if (currentDisplay.Length <= 1)
        {
            Buffer = 0;
            Exponent = 0;
            CurrentState = State.Number;
            return;
        }

        // Scenario A: We are removing the comma itself
        if (currentDisplay[^1] == ',')
        {
            Exponent = 0;
            CurrentState = State.Number;
            return;
        }
        // Scenario B: We are removing a digit
        else
        {
            if (Exponent < 0) Exponent++;

            string newDisplay = currentDisplay[..^1];

            // If removing the digit exposes the comma at the very end (e.g., "1,2" -> "1,")
            if (newDisplay[^1] == ',')
            {
                // Parse only the integer part
                Buffer = decimal.Parse(newDisplay[..^1]);
                CurrentState = State.Float;
            }
            else
            {
                // Parse safely
                Buffer = decimal.Parse(newDisplay);
                CurrentState = Exponent < 0 ? State.Float : State.Number;
            }
        }
    }


    // ######################################################
    // Helper Methods
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
