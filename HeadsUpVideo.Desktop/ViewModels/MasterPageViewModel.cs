using HeadsUpVideo.Desktop.Base;
using System;
using System.Collections.ObjectModel;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class MasterPageViewModel : NotifyPropertyChangedBase
    {
        public event EventHandler FileOpened;
        public event EventHandler RecentFilesUpdated;

        public Command OpenFileCmd { get; set; }
        
        public Command ClearRecentFilesCmd { get; set; }
        public Command<FileViewModel> OpenRecentFileCmd { get; set; }

        public FileViewModel CurrentFile { get; set; }
        public ObservableCollection<FileViewModel> RecentFiles { get; set; }

        public MasterPageViewModel()
        {
            OpenFileCmd = NavigationService.OpenNewFileCmd;
            ClearRecentFilesCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = ClearRecentFiles };
            OpenRecentFileCmd = NavigationService.OpenFileViewModelCmd;
        }

        public void Initialize()
        {
            LoadRecentFiles();
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
        }

        private void LoadRecentFiles()
        {
            RecentFiles = new ObservableCollection<FileViewModel>();

            foreach (var file in LocalIO.LoadRecentFileList())
            {
                RecentFiles.Add(new FileViewModel()
                {
                    ContentType = file.ContentType,
                    Name = file.Name,
                    Path = file.Path
                });
            }
        }

        private void ClearRecentFiles()
        {
            LocalIO.ClearRecentFiles();
            LoadRecentFiles();
        }

        private void RecentFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (RecentFilesUpdated != null)
                RecentFilesUpdated(this, new EventArgs());
        }
    }
}
