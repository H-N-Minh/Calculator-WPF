using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using YT.InputBox;

namespace YT
{
    public class DataForMainWindow : INotifyPropertyChanged
    {
        public DataClassForInputBox DataInputBox {  get; set; } = new DataClassForInputBox();

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return "not data for main window";
        }
    }
}
