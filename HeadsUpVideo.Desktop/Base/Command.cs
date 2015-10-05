using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeadsUpVideo.Desktop.Base
{
    public class Command<T> : ICommand where T : class
    {
        public event EventHandler CanExecuteChanged;

        public Predicate<object> CanExecuteFunc { get; set; }
        public Action<T> ExecuteFunc { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteFunc(parameter as T);
        }
    }

    public class Command : ICommand
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
