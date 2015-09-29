using System;
using System.Windows.Input;

namespace HeadsUpVideo.Desktop.Base
{
    public class EmptyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Predicate<object> CanExecuteFunc { get; set; }
        public Action ExecuteFunc { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteFunc();
        }
    }
}
