using System.ComponentModel;
using System.Windows.Input;

namespace WpfApp1
{
    public class SetTextCommand(ViewModel _viewModel) : ICommand
    {

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _viewModel.DisplayText != (string) parameter;
        }

        public void Execute(object? parameter)
        {
            
            _viewModel.DisplayText = (string) parameter;
        }

        public void Notify()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public SetTextCommand SetText { get; set; }
        public string DisplayText
        {
            get { return field; }
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayText"));
                SetText.Notify();
            }
        }


        public ViewModel()
        {
            SetText = new SetTextCommand(this);
            DisplayText = "Default text";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}