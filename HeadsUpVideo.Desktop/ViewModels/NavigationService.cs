using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Pages;
using HeadsUpVideo.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace HeadsUpVideo.Desktop.ViewModels
{
    internal static class NavigationService
    {
        private static Frame ContentFrame { get; set; }

        static NavigationService()
        {
            OpenFileViewModelCmd = new Command<FileViewModel>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileStream };
            OpenNewFileCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFile };
            OpenFileFromPathCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileFromPath };
            OpenFileViewModelCmd = new Command<FileViewModel>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileStream };
        }

        public static void Initialize(Frame contentFrame)
        {
            ContentFrame = contentFrame;
        }

        public static Command<FileViewModel> OpenFileViewModelCmd { get; set; }
        public static Command<string> OpenFileFromPathCmd { get; internal set; }
        public static Command OpenNewFileCmd { get; set; }
        
        private static void OpenFileStream(FileViewModel obj)
        {
            ContentFrame.Navigate(typeof(VideoPage), obj);
        }

        private static async void OpenFile()
        {
            var fileModel = await LocalIO.SelectAndOpenFile();

            if (fileModel == null)
                return;

            var fileVM = new FileViewModel()
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
            var file = await LocalIO.OpenFile(path, false);
            var newFile = new FileViewModel()
            {
                ContentType = file.ContentType,
                Path = file.Path,
                Name = file.Name,
                Stream = file.Stream
            };

            NavigationService.OpenFileViewModelCmd.Execute(newFile);
        }
    }
}
