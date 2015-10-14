using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HeadsUpVideo.Desktop.Models;
using Windows.Storage;
using System.Xml.Serialization;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.ApplicationModel;
using Windows.Storage.AccessCache;
using System.Collections.ObjectModel;
using HeadsUpVideo.Desktop.ViewModels;
using HeadsUpVideo.Desktop.Base;

namespace HeadsUpVideo.Desktop
{
    public static class StorageIO
    {
        public static ObservableCollection<FileModel> RecentFiles { get; set; }
        public static ObservableCollection<PenModel> QuickPens { get; set; }

        static StorageIO()
        {
            QuickPens = new ObservableCollection<PenModel>();
            RecentFiles = new ObservableCollection<FileModel>();

            ClearRecentFilesCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = ClearRecentFiles };

            LoadQuickPensCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = LoadQuickPens };
            SaveQuickPensCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = SaveQuickPens };
            ClearQuickPensCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = ClearQuickPens };
            AddQuickPenCmd = new Command<PenModel>() { CanExecuteFunc = obj => true, ExecuteFunc = AddQuickPen };

            LoadRecentFileList();
        }

        private static void AddQuickPen(PenModel newPen)
        {
            QuickPens.Add(new PenModel(newPen));
            SaveQuickPens();
        }

        private static void ClearQuickPens()
        {
            QuickPens.Clear();
            SaveQuickPens();
        }

        public static Command ClearRecentFilesCmd { get; set; }
        public static Command LoadQuickPensCmd { get; set; }
        public static Command SaveQuickPensCmd { get; set; }
        public static Command ClearQuickPensCmd { get; set; }
        public static Command<PenModel> AddQuickPenCmd { get; set; }

        private static async void LoadQuickPens()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new XmlSerializer(typeof(List<PenModel>));
            try
            {
                List<PenModel> quickPens;
                using (var fileStream = new FileStream(folder.Path + "\\quickPens.xml", FileMode.Open))
                {
                    quickPens = serializer.Deserialize(fileStream) as List<PenModel>;
                }

                QuickPens.Clear();

                foreach (var pen in quickPens)
                    QuickPens.Add(pen);
            }
            catch (Exception ex)
            {

                while (ex.InnerException != null)
                    ex = ex.InnerException;

                var dialog = new MessageDialog("Error loading quick pens list.  If this problem continues please contact support.\r\n" + ex.Message);
                await dialog.ShowAsync();

                QuickPens.Clear();
            }
        }

        private static async void SaveQuickPens()
        {
            var folder = ApplicationData.Current.LocalFolder;

            try
            {
                var serializer = new XmlSerializer(typeof(List<PenModel>));

                using (var fileStream = new FileStream(folder.Path + "\\quickPens.xml", FileMode.Create))
                {
                    serializer.Serialize(fileStream, QuickPens.ToList());
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                var dialog = new MessageDialog("Error saving quick pens list.  If this problem continues please contact support.\r\n" + ex.Message);
                await dialog.ShowAsync();
            }
        }
       
        private static void LoadRecentFileList()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new XmlSerializer(typeof(List<FileModel>));
            try
            {
                List<FileModel> recentFiles;
                using (var fileStream = new FileStream(folder.Path + "\\recent.xml", FileMode.Open))
                {
                    recentFiles = serializer.Deserialize(fileStream) as List<FileModel>;
                }

                RecentFiles.Clear();

                foreach (var file in recentFiles)
                    RecentFiles.Add(file);
            }
            catch
            {
                RecentFiles = new ObservableCollection<FileModel>();
            }
        }

        private static void ClearRecentFiles()
        {
            RecentFiles.Clear();
            SaveRecentFileList();
        }

        private static async void SaveRecentFileList()
        {
            var folder = ApplicationData.Current.LocalFolder;
          
            try
            {
                var serializer = new XmlSerializer(typeof(List<FileModel>));

                using (var fileStream = new FileStream(folder.Path + "\\recent.xml", FileMode.Create))
                {
                    serializer.Serialize(fileStream, RecentFiles.ToList());
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("Error saving recent files list.  If this problem continues please contact support.\r\n\r\n" + ex.Message);
                await dialog.ShowAsync();
            }
        }

        public static async Task<FileModel> SelectAndOpenFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".avi");

            var openFileResponse = await openPicker.PickSingleFileAsync();

            if (openFileResponse == null)
                return null;

            var result = await StorageIO.OpenFile(openFileResponse, true);

            return result;
        }

        public static async Task<FileModel> OpenFile(string path, bool addToRecentList)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);

            return await OpenFile(file, addToRecentList);
        }

        public static async Task<FileModel> OpenFile(StorageFile file, bool addToRecentList)
        {
            try
            {
                FileModel fileModel = new FileModel();
                fileModel.ContentType = file.ContentType;

                if (addToRecentList)
                {
                    if (!RecentFiles.Any(x => x.Path == file.Path))
                    {
                        RecentFiles.Add(new FileModel() { Path = file.Path, Name = file.Name });
                        SaveRecentFileList();
                        StorageApplicationPermissions.FutureAccessList.Add(file);
                    }
                }

                var openResponse = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                fileModel.Stream = openResponse;

                return fileModel;
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("There was an error opening the specified file.\r\n\r\n" + ex.Message, "Error opening file");
                await dialog.ShowAsync();
                return null;
            }
        }

    }
}
