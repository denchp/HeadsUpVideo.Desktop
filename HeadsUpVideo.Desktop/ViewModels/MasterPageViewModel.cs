using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Models;
using System;
using System.Collections.ObjectModel;
using Windows.Media.Capture;
using Windows.Storage;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class MasterPageViewModel : NotifyPropertyChangedBase
    {
        public event EventHandler FileOpened;

        public Command OpenFileCmd { get; set; }
        public Command ClearRecentFilesCmd { get; set; }
        public Command<FileModel> OpenRecentFileCmd { get; set; }
        public Command<String> OpenDiagramCmd { get; set; }
        public Command LiveReviewCmd { get; set; }
        public Command InstantReplayCmd { get; set; }

        private bool _topBarIsOpen = true;
        public bool TopBarIsOpen { get { return _topBarIsOpen; } set { _topBarIsOpen = value; TriggerPropertyChange("TopBarIsOpen"); } }

        public MasterPageViewModel()
        {
            OpenFileCmd = NavigationModel.OpenNewFileCmd;
            ClearRecentFilesCmd = StorageIO.ClearRecentFilesCmd;
            OpenRecentFileCmd = NavigationModel.OpenFileModelCmd;
            OpenDiagramCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenDiagram };
            LiveReviewCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenCamera };
            InstantReplayCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = NavigationModel.InstantReplay };
        }

        private void OpenDiagram(string obj)
        {
            TopBarIsOpen = false;
            NavigationModel.OpenDiagramImageCmd.Execute(obj);
        }

        private async void OpenCamera()
        {
            TopBarIsOpen = false;
            var dialog = new CameraCaptureUI();
            dialog.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;
            //dialog.VideoSettings.MaxDurationInSeconds = 45;
            dialog.VideoSettings.AllowTrimming = false;
            dialog.VideoSettings.MaxResolution = CameraCaptureUIMaxVideoResolution.HighestAvailable;
            
            StorageFile videoFile = null;
            videoFile = await dialog.CaptureFileAsync(CameraCaptureUIMode.Video);

            if (videoFile == null)
                return;

            var stream = await videoFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

            var fileModel = new FileModel() { Name = "Live", Stream = stream, ContentType = "video/mp4" };

            NavigationModel.OpenFileModelCmd.Execute(fileModel);
        }
        public void Initialize()
        {

        }
    }
}
