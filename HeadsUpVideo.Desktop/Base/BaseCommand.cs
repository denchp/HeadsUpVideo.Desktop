using HeadsUpVideo.Desktop.ViewModels;
using System;
using System.Windows.Input;

namespace HeadsUpVideo.Desktop.Base
{
    public class BaseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Predicate<object> CanExecuteFunc { get; set; }
        public Action<object> ExecuteFunc { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }
        
        public void Execute(object parameter)
        {
            ExecuteFunc(parameter);
        }
    }
}