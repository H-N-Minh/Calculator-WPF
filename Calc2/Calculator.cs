using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Calc2
{
    public enum Operator
    {
        Plus, Minus, Divide, Multiply
    }

    public class Calculator : INotifyPropertyChanged
    {
        // Fields
        private decimal result = 0;
        private decimal buffer;          // Part of property
        private int exponent;
        private Operator currentOperator = Operator.Plus;
        private bool isTyping = false;

        // Properties
        public bool IsTyping
        {
            get { return isTyping; }
            set
            {
                isTyping = value;
                OperatorCommandHandler.RaiseCanExecuteChanged();
            }
        }
        public Operator CurrentOperator
        {
            get { return currentOperator; }
            set
            {
                currentOperator = value;
            }
        }
        public int Exponent
        {
            get { return exponent; }
            set
            {
                exponent = value;
                OnPropertyChanged(nameof(Exponent));
                OnPropertyChanged(nameof(Display));
                CommaCommandHandler.RaiseCanExecuteChanged();
            }
        }

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
                if (Exponent == -1)
                {
                    return buffer.ToString() + ",";
                }
                else
                {
                    return buffer.ToString();
                }
            }
        }

        // Commands Property
        public RelayCommand DigitCommandHanlder { get; set; }
        public RelayCommand CommaCommandHandler { get; set; }
        public RelayCommand BackSpaceCommandHandler { get; set; }
        public RelayCommand ClearAllCommandHandler { get; set; }
        public RelayCommand OperatorCommandHandler { get; set; }
        public RelayCommand EqualCommandHandler { get; set; }

        // Event
        public event PropertyChangedEventHandler? PropertyChanged;


        /* ########################################################## */

        // Ctor
        public Calculator()
        {
            DigitCommandHanlder = new RelayCommand(enterDigit);
            CommaCommandHandler = new RelayCommand(enterComma, (parameter) => Exponent == 0);
            BackSpaceCommandHandler = new RelayCommand(enterBackSpace);
            ClearAllCommandHandler = new RelayCommand(enterClearAll);
            OperatorCommandHandler = new RelayCommand(enterOperator, (parameter) => !isStartState(parameter));
            EqualCommandHandler = new RelayCommand(enterEqual);
        }

        // Commands methods
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

        // Method
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}