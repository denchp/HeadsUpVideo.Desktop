using HeadsUpVideo.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeadsUpVideo.Desktop.Commands
{
    public class OpenRecentFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Predicate<object> CanExecuteFunc { get; set; }
        public Action<FileViewModel> ExecuteFunc { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteFunc(parameter as FileViewModel);
        }
    }
}
