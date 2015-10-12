using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Models;
using System;
using System.Collections.ObjectModel;

namespace HeadsUpVideo.Desktop.ViewModels
{
    internal class WelcomePageViewModel
    {
        public event EventHandler FileOpened;

        public  ObservableCollection<FileModel> RecentFiles { get; set; }
        public Command<string> OpenRecentFileCmd { get; set; }
        public Command ClearRecentFilesCmd { get; set; }

        public WelcomePageViewModel()
        {
            RecentFiles = StorageIO.RecentFiles;
            OpenRecentFileCmd = NavigationModel.OpenFileFromPathCmd;
            ClearRecentFilesCmd = StorageIO.ClearRecentFilesCmd;
        }

        
    }
}
