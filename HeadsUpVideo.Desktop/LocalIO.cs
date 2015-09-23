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

namespace HeadsUpVideo.Desktop
{
    public static class LocalIO
    {
        public static List<QuickPenModel> LoadQuickPens()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new XmlSerializer(typeof(List<QuickPenModel>));
            try
            {
                List<QuickPenModel> quickPens;
                using (var fileStream = new FileStream(folder.Path + "\\quickPens.xml", FileMode.Open))
                {
                    quickPens = serializer.Deserialize(fileStream) as List<QuickPenModel>;
                }

                if (quickPens == null)
                    return new List<QuickPenModel>();

                return quickPens;
            }
            catch
            {
                return new List<QuickPenModel>();
            }
        }

        internal async static void SaveQuickPens(List<QuickPenModel> currentPens)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new XmlSerializer(typeof(List<QuickPenModel>));

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

        internal static FileModel OpenFile(string path, bool addToRecentList)
        {
            try
            {
                var getResponse = StorageFile.GetFileFromPathAsync(path);
                StorageFile file = null;

                getResponse.Completed = (x, y) =>
                {
                    file = x.GetResults();
                };
                if (file == null)
                {
                    var dialog = new MessageDialog("There was an error opening the specified file.", "Error opening file");
                    dialog.ShowAsync();
                }

                return OpenFile(file, addToRecentList);
            }
            catch
            {
                var dialog = new MessageDialog("There was an error opening the specified file.", "Error opening file");
                dialog.ShowAsync();
            }

            return null;
        }
        internal static FileModel OpenFile(StorageFile file, bool addToRecentList)
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
                }

                var openResponse = file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                openResponse.Completed = (response, state) =>
                {
                    fileModel.Stream = response.GetResults();
                };

                return fileModel;
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("There was an error opening the specified file.\r\n\r\n" + ex.Message, "Error opening file");
                dialog.ShowAsync();
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

        internal static FileModel SelectAndOpenFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".avi");
            var openFileResponse = openPicker.PickSingleFileAsync();
            FileModel fileModel = null;

            openFileResponse.Completed = (result, status) =>
            {
                var path = result.GetResults();
                fileModel = LocalIO.OpenFile(result.GetResults(), true);
            };

            return fileModel;
        }
    }
}
