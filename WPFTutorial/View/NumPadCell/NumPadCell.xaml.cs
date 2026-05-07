using System;
using System.Collections.Generic;
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

namespace WPFTutorial.View.NumPadCell
{
    public partial class NumPadCell : UserControl
    {
        private Brush cellColor;

        public Brush CellColor
        {
            get { return cellColor; }
            set { cellColor = value; CellButt.Background = value; }
        }


        private string cellName;
        public string CellName {
            get { return cellName;  } 
            set { 
                cellName = value;
                CellButt.Content = value;
            }
        }
        public NumPadCell()
        {
            InitializeComponent();
        }
    }
}
