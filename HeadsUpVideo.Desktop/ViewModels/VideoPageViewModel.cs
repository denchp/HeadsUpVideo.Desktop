using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Commands;
using System.Collections.ObjectModel;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using HeadsUpVideo.Desktop.Interfaces;
using HeadsUpVideo.Desktop.Models;
using Windows.UI.Xaml;

namespace HeadsUpVideo.Desktop.ViewModels
{
    class VideoPageViewModel : MasterPageViewModel
    {
        public event EventHandler TogglePlayPauseEvent;
        public event EventHandler RecentFilesUpdated;

        public ICustomCanvas Canvas { get; set; }

        public PenViewModel CurrentPen { get; set; }
        
        public ObservableCollection<PenViewModel> QuickPens { get; set; }
        public ObservableCollection<FileViewModel> RecentFiles { get; set; }
        public TimeSpan LastVideoPosition { get; set; }

        public SetLineStyleCommand SetLineStyleCmd { get; set; }
        public ClearQuickPensCommand ClearQuickPensCmd { get; set; }
        public SaveQuickPenCommand SaveQuickPenCmd { get; set; }
        public LoadQuickPenCommand LoadQuickPenCmd { get; set; }
        public PlayPauseCommand PlayPauseCmd { get; set; }
        public ClearRecentFilesCommand ClearRecentFilesCmd { get; set; }
        public ClearLinesCommand ClearLinesCmd { get; set; }
        public CreateSavePointCommand CreateSavePointCmd { get; set; }

        private Visibility welcomeScreenVisibility;
        public Visibility WelcomeScreenVisibility
        {
            get { return welcomeScreenVisibility; }
            set { welcomeScreenVisibility = value; TriggerPropertyChange("WelcomeScreenVisibility"); }
        }

        private Visibility diagramVisibility;
        public Visibility DiagramVisibility
        {
            get { return diagramVisibility; }
            set { diagramVisibility = value; TriggerPropertyChange("DiagramVisibility"); }
        }

        public VideoPageViewModel()
        {
            CurrentPen = new PenViewModel()
            {
                IsFreehand = true,
                Color = Colors.Blue,
                LineStyle = PenViewModel.LineType.Solid,
                Size = 10
            };

            CurrentFile = new FileViewModel();
            QuickPens = new ObservableCollection<PenViewModel>();
            RecentFiles = new ObservableCollection<FileViewModel>();
            
            SetLineStyleCmd = new SetLineStyleCommand() { CanExecuteFunc = obj => true, ExecuteFunc = SetLineStyle };
            ClearQuickPensCmd = new ClearQuickPensCommand() { CanExecuteFunc = obj => true, ExecuteFunc = ClearQuickPens };
            SaveQuickPenCmd = new SaveQuickPenCommand() { CanExecuteFunc = obj => true, ExecuteFunc = SaveQuickPen };
            LoadQuickPenCmd = new LoadQuickPenCommand() { CanExecuteFunc = obj => true, ExecuteFunc = LoadQuickPen };
            PlayPauseCmd = new PlayPauseCommand() { CanExecuteFunc = obj => true, ExecuteFunc = TogglePlayPause };
            ClearRecentFilesCmd = new ClearRecentFilesCommand() { CanExecuteFunc = obj => true, ExecuteFunc = ClearRecentFiles };
            CreateSavePointCmd = new CreateSavePointCommand() { CanExecuteFunc = obj => true, ExecuteFunc = CreateSavePoint };
            ClearLinesCmd = new ClearLinesCommand() { CanExecuteFunc = obj => true, ExecuteFunc = ClearLines };
        }

        public void Initialize(ICustomCanvas canvas)
        {
            RecentFiles = new ObservableCollection<FileViewModel>();
            RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;
            Canvas = canvas;
            WelcomeScreenVisibility = Visibility.Visible;

            LoadRecentFiles();
            RefreshQuickPens();
        }

        private void ClearLines()
        {
            Canvas.Clear();
        }

        private void CreateSavePoint()
        {
            Canvas.CreateSavePoint();
        }

        private void RecentFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (RecentFilesUpdated != null)
                RecentFilesUpdated(this, new EventArgs());
        }

        private void ClearRecentFiles()
        {
            LocalIO.ClearRecentFiles();
            LoadRecentFiles();
        }

        private void TogglePlayPause()
        {
            Canvas.ClearLines(false, false);

            if (TogglePlayPauseEvent != null)
                TogglePlayPauseEvent(this, new EventArgs());
        }

        private void LoadQuickPen(PenViewModel obj)
        {
            CurrentPen = obj;
            Canvas.SetPen(CurrentPen);
        }

        private void ClearQuickPens(object obj)
        {
            LocalIO.SaveQuickPens();
            RefreshQuickPens();
        }

        private void SaveQuickPen(object obj)
        {
            QuickPens.Add(CurrentPen);

            var tempList = new List<PenModel>();

            foreach (var pen in QuickPens)
                tempList.Add(new PenModel()
                {
                    Color = pen.Color,
                    Size = pen.Size,
                    EnableArrow = pen.EnableArrow,
                    IsFreehand = pen.IsFreehand,
                    IsHighlighter = pen.IsHighlighter,
                    LineStyle = pen.LineStyle
                });

            LocalIO.SaveQuickPens(tempList);
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
                    Size = pen.Size
                });
            }
        }

    }
}
