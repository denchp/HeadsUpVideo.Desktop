using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Models;
using System;
using System.Collections.ObjectModel;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class MasterPageViewModel : NotifyPropertyChangedBase
    {
        public event EventHandler FileOpened;

        public Command OpenFileCmd { get; set; }
        public Command ClearRecentFilesCmd { get; set; }
        public Command<FileModel> OpenRecentFileCmd { get; set; }
        public Command<String> OpenDiagramCmd { get; set; }
        
        public MasterPageViewModel()
        {
            OpenFileCmd = NavigationModel.OpenNewFileCmd;
            ClearRecentFilesCmd = StorageIO.ClearRecentFilesCmd;
            OpenRecentFileCmd = NavigationModel.OpenFileModelCmd;
            OpenDiagramCmd = NavigationModel.OpenDiagramImageCmd;
        }

        public void Initialize()
        {

        }
    }
}
