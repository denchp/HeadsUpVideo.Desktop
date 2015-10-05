using HeadsUpVideo.Desktop.Base;
using System;
using System.Collections.ObjectModel;

namespace HeadsUpVideo.Desktop.ViewModels
{
    internal class WelcomePageViewModel
    {
        public event EventHandler FileOpened;

        public  ObservableCollection<FileViewModel> RecentFiles { get; set; }
        public Command<string> OpenRecentFileCmd { get; set; }

        public WelcomePageViewModel()
        {
            RecentFiles = new ObservableCollection<FileViewModel>();
            OpenRecentFileCmd = NavigationService.OpenFileFromPathCmd;
        }

        
    }
}
