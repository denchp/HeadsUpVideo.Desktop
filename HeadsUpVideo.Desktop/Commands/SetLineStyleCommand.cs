using System;
using HeadsUpVideo.Desktop.Base;
using System.Windows.Input;

namespace HeadsUpVideo.Desktop.Commands
{
    public class SetLineStyleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Predicate<object> CanExecuteFunc { get; set; }
        public Action<string> ExecuteFunc { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteFunc(parameter as string);
        }
    }
}
