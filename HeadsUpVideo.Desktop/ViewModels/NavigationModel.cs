using System;
using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Models;
using HeadsUpVideo.Desktop.Pages;
using Windows.UI.Xaml.Controls;

namespace HeadsUpVideo.Desktop.ViewModels
{
    internal static class NavigationModel
    {
        private static Frame ContentFrame { get; set; }

        static NavigationModel()
        {
            OpenFileModelCmd = new Command<FileModel>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileStream };
            OpenNewFileCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFile };
            OpenFileFromPathCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileFromPath };
            OpenFileModelCmd = new Command<FileModel>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileStream };
            OpenDiagramImageCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenDiagramImage };
        }

        public static void Initialize(Frame contentFrame)
        {
            ContentFrame = contentFrame;

            ContentFrame.Navigate(typeof(WelcomePage));
        }

        public static Command<FileModel> OpenFileModelCmd { get; set; }
        public static Command<string> OpenFileFromPathCmd { get; set; }
        public static Command OpenNewFileCmd { get; set; }
        public static Command<string> OpenDiagramImageCmd { get; set; }

        private static void OpenFileStream(FileModel obj)
        {
            ContentFrame.Navigate(typeof(VideoPage), obj);
        }

        private static async void OpenFile()
        {
            var fileModel = await StorageIO.SelectAndOpenFile();

            if (fileModel == null)
                return;

            var fileVM = new FileModel()
            {
                ContentType = fileModel.ContentType,
                Name = fileModel.Name,
                Path = fileModel.Path,
                Stream = fileModel.Stream
            };

            OpenFileStream(fileVM);
        }

        private static async void OpenFileFromPath(string path)
        {
            var file = await StorageIO.OpenFile(path, false);
            var newFile = new FileModel()
            {
                ContentType = file.ContentType,
                Path = file.Path,
                Name = file.Name,
                Stream = file.Stream
            };

            NavigationModel.OpenFileModelCmd.Execute(newFile);
        }


        private static void OpenDiagramImage(string obj)
        {
            ContentFrame.Navigate(typeof(DiagramPage), obj);
        }
    }
}
