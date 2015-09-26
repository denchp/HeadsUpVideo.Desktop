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

namespace HeadsUpVideo.Desktop
{
    public static class LocalIO
    {
        public static List<PenModel> LoadQuickPens()
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

                if (quickPens == null)
                    return new List<PenModel>();

                return quickPens;
            }
            catch
            {
                return new List<PenModel>();
            }
        }

        internal static async void SaveQuickPens(IEnumerable<PenModel> currentPens = null)
        {
            if (currentPens == null)
                currentPens = new List<PenModel>();

            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new XmlSerializer(typeof(List<PenModel>));

                using (var fileStream = new FileStream(folder.Path + "\\quickPens.xml", FileMode.Create))
                {
                    serializer.Serialize(fileStream, currentPens);
                }
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("Error saving quick pens list.  If this problem continues please contact support.");
                await dialog.ShowAsync();
            }
        }

        internal static async Task<FileModel> OpenFile(string path, bool addToRecentList)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            
            return await OpenFile(file, addToRecentList);
        }

        internal static async Task<FileModel> OpenFile(StorageFile file, bool addToRecentList)
        {
            try
            {
                FileModel fileModel = new FileModel();
                fileModel.ContentType = file.ContentType;

                if (addToRecentList)
                {
                    var fileList = LoadRecentFileList();
                    fileList.Add(new FileModel() { Path = file.Path, Name = file.Name });
                    SaveRecentFileList(fileList);
                    StorageApplicationPermissions.FutureAccessList.Add(file);
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

        internal static List<FileModel> LoadRecentFileList()
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

                if (recentFiles == null)
                    return new List<FileModel>();

                return recentFiles;
            }
            catch
            {
                return new List<FileModel>();
            }
        }

        private static async void SaveRecentFileList(List<FileModel> files)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new XmlSerializer(typeof(List<FileModel>));

                using (var fileStream = new FileStream(folder.Path + "\\recent.xml", FileMode.Create))
                {
                    serializer.Serialize(fileStream, files);
                }
            }
            catch
            {
                var dialog = new MessageDialog("Error saving recent files list.  If this problem continues please contact support.");
                await dialog.ShowAsync();
            }
        }

        internal static async Task<FileModel> SelectAndOpenFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".avi");

            var openFileResponse = await openPicker.PickSingleFileAsync();
            
            var result = await LocalIO.OpenFile(openFileResponse, true);

            return result;
        }
    }
}
