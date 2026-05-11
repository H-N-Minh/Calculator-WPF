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
        Debug.Assert(CurrentState != operatorState, "Equal cant be entered after an operator");
        Debug.Assert(CurrentState != startState, "Equal cant be entered at the start");
        Debug.Assert(!(CurrentOperator == Operator.Divide && Buffer == 0), "Equal cant be entered for division by 0");

        // If only 1 number entered, pressing equal repetively should not change result.
        bool isOnlyBufferHasNumber = result == 0 && CurrentOperator == Operator.Plus && (CurrentState == numberState || CurrentState == floatState);

        // Updating fields and properties
        result = CalculateResult();
        Exponent = 0;
        if (isOnlyBufferHasNumber) Buffer = 0;
        CurrentState = equalState;
    }

    private void EnterOperator(string arithOperator)
    {
        Debug.Assert(CurrentState != startState, "Operator cant be entered at the start");

        // If operator is first entered after a number, calculate the result of the previous operation
        if (CurrentState == numberState || CurrentState == floatState)
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
        CurrentState = operatorState;
    }

    private void EnterClearAll()
    {
        result = 0;
        Buffer = 0;
        Exponent = 0;
        CurrentOperator = Operator.Plus;
        CurrentState = startState;
    }

    private void EnterComma()
    {
        Debug.Assert(CurrentState != floatState, "Comma cant be added to a float");
        Debug.Assert(CurrentState != operatorState, "Comma cant be entered after an operator");
        Debug.Assert(CurrentState != equalState, "Comma cant be entered after Equal button");

        Exponent--;
        CurrentState = floatState;
    }

    private void EnterDigit(decimal digit)
    {
        if (CurrentState == equalState)    EnterClearAll();

        Buffer = Exponent switch
        {
            0 => Buffer * 10 + digit,
            _ => Buffer + digit * (decimal)Math.Pow(10, Exponent--)
        };

        if (CurrentState == floatState)
        {
            CurrentState = floatState;
        }
        else
        {
            CurrentState = numberState;
        }
    }

    private void EnterBackSpace()
    {
        Debug.Assert(CurrentState != startState, "Backspace cant be entered at the start");
        Debug.Assert(CurrentState != operatorState, "Backspace cant be entered after an operator");
        Debug.Assert(CurrentState != equalState, "Backspace cant be entered after Equal button");

        string currentDisplay = Display;

        // If there is only 1 character left, reset
        if (currentDisplay.Length <= 1)
        {
            Buffer = 0;
            Exponent = 0;
            bool isStartState = result == 0 && CurrentOperator == Operator.Plus;
            CurrentState = isStartState ? startState : numberState;
            return;
        }

        string newDisplay = currentDisplay[..^1];
        if (CurrentState == numberState)
        {
            Buffer = decimal.Parse(newDisplay);
            Exponent = 0;
            CurrentState = numberState;
        }
        else if (CurrentState == floatState)
        {
            // If removing the digit exposes the comma at the very end (e.g., "1,2" -> "1,")
            if (newDisplay[^1] == ',')
            {
                Buffer = decimal.Parse(newDisplay[..^1]);
                Exponent = -1;
                CurrentState = floatState;
            }
            else // If removing the digit expose a digit at the very end (e.g., "1,23" -> "1,2")
            {
                Buffer = decimal.Parse(newDisplay);
                Exponent++;
                CurrentState = Exponent < 0 ? floatState : numberState;
            }
        }
        else
        {
            throw new NotImplementedException("Unknown state. Expected FloatState or NumberState");
        }
    }

    private void EnterClearEntry()
    {
        Debug.Assert(CurrentState != operatorState, "Clear Entry cant be entered after an operator");

        bool isTypingNumber = CurrentState == numberState || CurrentState == floatState;
        if (isTypingNumber)
        {
            Buffer = 0;
            Exponent = 0;
            CurrentState = numberState;
        }
        else
        {
            EnterClearAll();
        }
    }

    private void EnterSignFlip()
    {
        Debug.Assert(CurrentState != startState, "Sign flip cant be entered at the start");
        Debug.Assert(CurrentState != operatorState, "Sign flip cant be entered after an operator");

        if (CurrentState == equalState)
        {
            result = -result;
            OnPropertyChanged(nameof(Display));
        }
        else
        {
            Buffer = -Buffer;
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
