using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Commands;
using System.Collections.ObjectModel;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using HeadsUpVideo.Desktop.Interfaces;
using HeadsUpVideo.Desktop.Models;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class MainPageViewModel : NotifyPropertyChangedBase
    {
        public ICustomCanvas Canvas { get; set; }
        public MediaElement VideoPlayer { get; set; }

        public PenViewModel CurrentPen { get; set; }
        public FileViewModel CurrentFile { get; set; }
        public ObservableCollection<PenViewModel> QuickPens { get; set; }
        public ObservableCollection<FileViewModel> RecentFiles { get; set; }
        public TimeSpan LastVideoPosition { get; set; }

        public BaseCommand OpenFileCmd { get; set; }
        public SetLineStyleCommand SetLineStyleCmd { get; set; }
        public BaseCommand ClearQuickPensCmd { get; set; }
        public BaseCommand SaveQuickPenCmd { get; set; }
        public LoadQuickPenCommand LoadQuickPenCmd { get; set; }
        public PlayPauseCommand PlayPauseCmd { get; set; }

        private Visibility welcomeScreenVisible;
        public Visibility WelcomeScreenVisible
        {
            get { return welcomeScreenVisible; }
            set { welcomeScreenVisible = value; TriggerPropertyChange("WelcomeScreenVisible"); }
        }

        public MainPageViewModel()
        {
            CurrentPen = new PenViewModel();
            CurrentFile = new FileViewModel();
            QuickPens = new ObservableCollection<PenViewModel>();
            RecentFiles = new ObservableCollection<FileViewModel>();
        }

        public void Initialize(ICustomCanvas canvas, MediaElement videoPlayer)
        {
            CurrentPen = new PenViewModel()
            {
                IsFreehand = true,
                Color = Colors.Blue,
                LineStyle = PenViewModel.LineType.Solid,
                Size = 10
            };
            Canvas = canvas;
            VideoPlayer = videoPlayer;
            WelcomeScreenVisible = Visibility.Visible;

            LoadRecentFiles();
            OpenFileCmd = new OpenFileCommand() { CanExecuteFunc = obj => true, ExecuteFunc = OpenFile };
            SetLineStyleCmd = new SetLineStyleCommand() { CanExecuteFunc = obj => true, ExecuteFunc = SetLineStyle };
            ClearQuickPensCmd = new ClearQuickPensCommand() { CanExecuteFunc = obj => true, ExecuteFunc = ClearQuickPens };
            SaveQuickPenCmd = new SaveQuickPenCommand() { CanExecuteFunc = obj => true, ExecuteFunc = SaveQuickPen };
            LoadQuickPenCmd = new LoadQuickPenCommand() { CanExecuteFunc = obj => true, ExecuteFunc = LoadQuickPen };
            PlayPauseCmd = new PlayPauseCommand() { CanExecuteFunc = obj => true, ExecuteFunc = TogglePlayPause };
        }

        private void TogglePlayPause()
        {
            Canvas.ClearLines(false, false);

            switch (VideoPlayer.CurrentState)
            {
                case MediaElementState.Stopped:
                case MediaElementState.Playing:
                    VideoPlayer.Pause();
                    LastVideoPosition = VideoPlayer.Position;
                    break;
                case MediaElementState.Paused:
                    VideoPlayer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    VideoPlayer.Play();
                    break;
            }


        }

        private void LoadQuickPen(PenViewModel obj)
        {
            CurrentPen = obj;
        }

        private void ClearQuickPens(object obj)
        {
            LocalIO.SaveQuickPens();
            RefreshQuickPens();
        }

        private void SaveQuickPen(object obj)
        {
            QuickPens.Add(CurrentPen);

            LocalIO.SaveQuickPens(QuickPens.Cast<PenModel>());
            RefreshQuickPens();
        }

        private async void OpenFile(object obj)
        {
            var fileModel = await LocalIO.SelectAndOpenFile();
            CurrentFile = new FileViewModel() {
                ContentType = fileModel.ContentType,
                Name = fileModel.Name,
                Path = fileModel.Path,
                Stream = fileModel.Stream
            };

            WelcomeScreenVisible = Visibility.Collapsed; 
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

        public void SetLineStyle(string style)
        {
           switch (style)
            {
                case "Solid":
                    CurrentPen.LineStyle = Models.PenModel.LineType.Solid;
                    break;
                case "Dash":
                    CurrentPen.LineStyle = Models.PenModel.LineType.Dashed;
                    break;

                case "Blue":
                    CurrentPen.Color = Colors.Blue;
                    break;
                case "Red":
                    CurrentPen.Color = Colors.Red;
                    break;
                case "Yellow":
                    CurrentPen.Color = Colors.Yellow;
                    break;
                case "Green":
                    CurrentPen.Color = Colors.Green;
                    break;
                case "Black":
                    CurrentPen.Color = Colors.Black;
                    break;

                case "Highlight":
                    CurrentPen.IsHighlighter = !CurrentPen.IsHighlighter;
                    break;


                case "Small":
                    CurrentPen.Size = 5;
                    break;
                case "Medium":
                    CurrentPen.Size = 10;
                    break;
                case "Large":
                    CurrentPen.Size = 20;
                    break;

                case "Arrow":
                    CurrentPen.EnableArrow = !CurrentPen.EnableArrow;
                    break;
            }
        }

        private void RefreshQuickPens()
        {
            QuickPens.Clear();

            foreach (var pen in LocalIO.LoadQuickPens())
            {
                QuickPens.Add(new PenViewModel()
                {
                    Color = pen.Color,
                    EnableArrow = pen.EnableArrow,
                    IsFreehand = pen.IsFreehand,
                    IsHighlighter = pen.IsHighlighter,
                    LineStyle = pen.LineStyle,
                    Size =pen.Size
                });
            }
        }

    }
}
