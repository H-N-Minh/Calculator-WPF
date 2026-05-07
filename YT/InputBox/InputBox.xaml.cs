using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YT.InputBox
{

    public class DataClassForInputBox : INotifyPropertyChanged, ICommand
    {
        private string inputText = "";
        private string blockText = "Type here...";

        public string InputText
        {
            get { return inputText; }
            set { 
                inputText = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputText)));
                blockText = inputText.Length > 0 ? "" : "Type here...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlockText)));
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CanExecuteChanged;

        public string BlockText
        {
            get { return blockText; }
            set { blockText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BlockText")); }
        }

        public bool CanExecute(object? parameter)
        {
            return InputText != "";
        }

        public void Execute(object? parameter)
        {
            InputText = "";
        }
    }

    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : UserControl
    {
        

        public InputBox()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataClassForInputBox data = (DataClassForInputBox) DataContext;
        }
    }
}
