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
        public static OptionsModel Options { get; set; }

        static NavigationModel()
        {
            Options = StorageIO.LoadOptions();

            OpenFileModelCmd = new Command<FileModel>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileStream };
            OpenNewFileCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFile };
            OpenFileFromPathCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileFromPath };
            OpenFileModelCmd = new Command<FileModel>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFileStream };
            OpenDiagramImageCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenDiagramImage };
            Media = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenMediaPage };
            BreakdownCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = OpenBreakdown };
            OpenBreakdownFromPathCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = OpenBreakdownFromPath };
        }

        private static async void OpenBreakdownFromPath(string path)
        {
            var file = await StorageIO.OpenBreakdown(path);
            var breakdown = new BreakdownModel();

            foreach (var instance in file.Instances)
                breakdown.Instances.Add(new BreakdownModel.Instance()
                {
                    Category = instance.Category,
                    Id = instance.Id,
                    Label = instance.Label,
                    StartTime = instance.StartTime,
                    StopTime = instance.StopTime
                });

            file.AssociatedVideo = new FileModel()
            {
                ContentType = breakdown.AssociatedVideo.ContentType,
                Name = breakdown.AssociatedVideo.Name,
                Path = breakdown.AssociatedVideo.Path,
                Stream = breakdown.AssociatedVideo.Stream
            };

            OpenBreakdown(breakdown);
        }
        private static async void OpenBreakdown()
        {
            ContentFrame.Navigate(typeof(BreakdownPage));
            return;
            var breakdown = await StorageIO.OpenBreakdown();

            if (breakdown == null)
                return;

            var breakdownVM = new BreakdownModel();

            foreach (var instance in breakdown.Instances)
                breakdownVM.Instances.Add(new BreakdownModel.Instance()
                {
                    Category = instance.Category,
                    Id = instance.Id,
                    Label = instance.Label,
                    StartTime = instance.StartTime,
                    StopTime = instance.StopTime
                });

            breakdownVM.AssociatedVideo = new FileModel()
            {
                ContentType = breakdown.AssociatedVideo.ContentType,
                Name = breakdown.AssociatedVideo.Name,
                Path = breakdown.AssociatedVideo.Path,
                Stream = breakdown.AssociatedVideo.Stream
            };

            OpenBreakdown(breakdownVM);
        }

        private static void OpenBreakdown(BreakdownModel obj)
        {
            ContentFrame.Navigate(typeof(BreakdownPage), obj);
        }

        private static void OpenMediaPage()
        {
            ContentFrame.Navigate(typeof(WelcomePage));
        }

        public static void Initialize(Frame contentFrame)
        {
            ContentFrame = contentFrame;

            ContentFrame.Navigate(typeof(WelcomePage));
        }

        public static Command<FileModel> OpenFileModelCmd { get; set; }
        public static Command<string> OpenFileFromPathCmd { get; set; }
        public static Command OpenNewFileCmd { get; set; }

        internal static void InstantReplay()
        {
            ContentFrame.Navigate(typeof(InstantReplayPage));
        }

        public static Command<string> OpenDiagramImageCmd { get; set; }
        public static Command LiveReviewCmd { get; set; }
        public static Command Media { get; internal set; }
        public static Command BreakdownCmd { get; internal set; }
        public static Command<string> OpenBreakdownFromPathCmd { get; private set; }

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
