using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Commands;
using HeadsUpVideo.Desktop.Pages;
using System;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class MasterPageViewModel : NotifyPropertyChangedBase
    {
        public event EventHandler FileOpened;
        public event EventHandler<System.Type> NavigateTo;

        public OpenFileCommand OpenFileCmd { get; set; }
        public OpenRecentFileCommand OpenRecentFileCmd { get; set; }

        private FileViewModel currentFile;
        public FileViewModel CurrentFile
        {
            get { return currentFile; }
            set { currentFile = value; TriggerPropertyChange("CurrentFile"); }
        }

        public MasterPageViewModel()
        {

            OpenFileCmd = new OpenFileCommand() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFile };
            OpenRecentFileCmd = new OpenRecentFileCommand() { CanExecuteFunc = obj => true, ExecuteFunc = OpenRecentFile };
        }

        public void Initialize()
        {
            if (NavigateTo != null)
                NavigateTo(this, typeof(WelcomePage));
        }

        private async void OpenFile()
        {
            var fileModel = await LocalIO.SelectAndOpenFile();

            if (fileModel == null)
                return;

            CurrentFile = new FileViewModel()
            {
                ContentType = fileModel.ContentType,
                Name = fileModel.Name,
                Path = fileModel.Path,
                Stream = fileModel.Stream
            };

            if (FileOpened != null)
                FileOpened(this, new EventArgs());

            if (NavigateTo != null)
                NavigateTo(this, typeof(VideoPage));
        }

        private async void OpenRecentFile(FileViewModel recentFile)
        {
            var file = await LocalIO.OpenFile(recentFile.Path, false);
            CurrentFile = new FileViewModel()
            {
                ContentType = file.ContentType,
                Path = file.Path,
                Name = file.Name,
                Stream = file.Stream
            };

            if (FileOpened != null)
                FileOpened(this, new EventArgs());

            if (NavigateTo != null)
                NavigateTo(this, typeof(VideoPage));
        }
    }
}
