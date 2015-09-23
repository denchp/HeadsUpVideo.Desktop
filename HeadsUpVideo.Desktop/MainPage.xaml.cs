using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input.Inking;
using Windows.UI;
using HeadsUpVideo.Desktop.Models;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;

namespace HeadsUpVideo.Desktop
{
    public sealed partial class MainPage : Page
    {
        InkDrawingAttributes _inkDA;
        TimeSpan lastVideoPosition;
        List<Path> _savePoint;
        List<Path> _activeTemplate;
        List<Path> _lines;
        List<Polyline> _polylines;

        InkSynchronizer _synch;
        QuickPenModel _currentPen;

        public MainPage()
        {
            this.InitializeComponent();
            _currentPen = new QuickPenModel()
            {
                IsFreehand = true
            };

            _activeTemplate = new List<Path>();
            _savePoint = new List<Path>();
            _lines = new List<Path>();
            _polylines = new List<Polyline>();

            _synch = inkCanvas.InkPresenter.ActivateCustomDrying();

            _inkDA = new InkDrawingAttributes()
            {
                Color = Colors.Blue,
                FitToCurve = false,
                Size = new Size(10, 10),
                IgnorePressure = true
            };

            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(_inkDA);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;

            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            btnClear.Tapped += BtnClear_Tapped;
            btnOpen.Tapped += BtnOpen_Tapped;
            btnPlay.Tapped += BtnPlay_Tapped;
            BtnSavePoint.Tapped += BtnSavePoint_Tapped;
            btnMore.Tapped += BtnMore_Tapped;
            btnMoreControls.Tapped += BtnMoreControls_Tapped;
            btnFullIce.Tapped += BtnFullIce_Tapped;
            btnHalfIce.Tapped += BtnHalfIce_Tapped;
            btnCurrentToQuick.Tapped += BtnCurrentToQuick_Tapped;
            btnClearQuickPens.Tapped += BtnClearQuickPens_Tapped;
            BtnRecentFiles.Tapped += BtnRecentFiles_Tapped;
            Scrubber.ValueChanged += Scrubber_ValueChanged;

            foreach (var file in LocalIO.LoadRecentFileList())
            {
                var newLink = new HyperlinkButton();
                var shortLink = new HyperlinkButton();
                newLink.Content = file.Path;
                newLink.DataContext = file;

                shortLink.Content = file.Name;
                shortLink.DataContext = file;
                newLink.Tapped += OpenFile_HyperlinkButton_Handler;
                shortLink.Tapped += OpenFile_HyperlinkButton_Handler;

                lstRecentFiles.Children.Add(newLink);
                pnlRecentDropDown.Children.Add(shortLink);
            }
            RefreshQuickPens();

            this.Loaded += MainPage_Loaded;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            var strokes = _synch.BeginDry();

            RenderStrokes(strokes);

            _synch.EndDry();
        }

        private void RenderStrokes(IReadOnlyList<InkStroke> strokes)
        {
            foreach (var s in strokes)
            {
                Point bearingPoint;
                Point lastPoint;
                var points = new List<Point>();
                int includeInterval = 10;
                int includePoint = includeInterval;

                foreach (var p in s.GetInkPoints())
                {
                    lastPoint = new Point(p.Position.X, p.Position.Y);

                    if (includePoint % includeInterval == 0)
                    {
                        bearingPoint = lastPoint;
                        lastPoint = new Point(p.Position.X, p.Position.Y);
                        points.Add(lastPoint);
                    }

                    includePoint++;
                }

                if (includePoint % includeInterval != 0) // make sure we don't chop off the end of the line!
                    points.Add(lastPoint);

                if (!_currentPen.IsFreehand)
                    points = new List<Point> { points[0], points[points.Count - 1] };

                // Get Bezier Spline Control Points.
                Point[] cp1, cp2;
                LineHelpers.GetCurveControlPoints(points.ToArray(), out cp1, out cp2);

                // Draw curve by Bezier.
                PathSegmentCollection lines = new PathSegmentCollection();
                for (int i = 0; i < cp1.Length; ++i)
                {
                    var bezierSeg = new BezierSegment();
                    bezierSeg.Point1 = cp1[i];
                    bezierSeg.Point2 = cp2[i];
                    bezierSeg.Point3 = points[i + 1];

                    lines.Add(bezierSeg);
                }

                PathFigure fig = new PathFigure()
                {
                    StartPoint = points[0],
                    Segments = lines,
                    IsClosed = false,
                };

                PathGeometry geometry = new PathGeometry();
                geometry.Figures.Add(fig);

                var path = new Path()
                {
                    Stroke = new SolidColorBrush(_inkDA.Color),
                    StrokeThickness = _inkDA.Size.Width,
                    StrokeEndLineCap = PenLineCap.Triangle,
                    Data = geometry
                };

                var arrowheadPath = new Polyline();
                arrowheadPath.Fill = new SolidColorBrush(Colors.Black);

                foreach (var p in LineHelpers.DrawArrow(bearingPoint, lastPoint, _inkDA.Size.Width * 2))
                    arrowheadPath.Points.Add(p);

                if (_currentPen.LineStyle == QuickPenModel.LineType.Dashed)
                    path.StrokeDashArray = new DoubleCollection() { 5, 2 };

                _lines.Add(path);
                _polylines.Add(arrowheadPath);
                LineCanvas.Children.Clear();

                foreach (var line in _lines)
                    LineCanvas.Children.Add(line);

                foreach (var polyline in _polylines)
                    LineCanvas.Children.Add(polyline);

            }
        }

        
        private void BtnSavePoint_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _savePoint = new List<Path>();

            foreach (var line in _lines)
            {
                _savePoint.Add(line);
            }
        }

        private void BtnRecentFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlRecentDropDown.Visibility = Visibility.Visible;
        }

        private void RefreshQuickPens()
        {
            QuickPens.Children.Clear();

            foreach (var pen in LocalIO.LoadQuickPens())
            {
                var quickPen = new AppBarButton();
                quickPen.Foreground = new SolidColorBrush(pen.Color);

                quickPen.Icon = new SymbolIcon(Symbol.Edit);
                if (pen.IsHighlighter)
                    quickPen.Icon = new SymbolIcon(Symbol.Highlight);

                quickPen.BorderThickness = new Thickness(pen.Size, pen.Size, pen.Size, pen.Size);

                quickPen.Tapped += QuickPen_Tapped;
                QuickPens.Children.Add(quickPen);
            }
        }

        private void QuickPen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pen = sender as AppBarButton;

            _inkDA.Color = ((SolidColorBrush)pen.Foreground).Color;
            _inkDA.DrawAsHighlighter = false;
            _inkDA.Size = new Size(pen.BorderThickness.Top, pen.BorderThickness.Top);

            if (((SymbolIcon)pen.Icon).Symbol == Symbol.Highlight)
                _inkDA.DrawAsHighlighter = true;

            UpdateInk(_inkDA);
        }


        private void BtnClearQuickPens_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalIO.SaveQuickPens(new List<QuickPenModel>());

            RefreshQuickPens();
        }

        private void BtnCurrentToQuick_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var currentPens = LocalIO.LoadQuickPens();

            currentPens.Add(_currentPen);
            LocalIO.SaveQuickPens(currentPens);

            RefreshQuickPens();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var thumb = MyFindSliderChildOfType<Thumb>(Scrubber);

            thumb.DragCompleted += Thumb_DragCompleted;

            pnlControls.Visibility = Visibility.Collapsed;

            var view = ApplicationView.GetForCurrentView();
            if (!view.TryEnterFullScreenMode())
            {
                var dialog = new MessageDialog("Unable to enter the full-screen mode.");
                await dialog.ShowAsync();
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Scrubber.Value = 50;
            lastVideoPosition = videoPlayer.Position;
        }

        public static T MyFindSliderChildOfType<T>(DependencyObject root) where T : class
        {
            var MyQueue = new Queue<DependencyObject>();
            MyQueue.Enqueue(root);
            while (MyQueue.Count > 0)
            {
                DependencyObject current = MyQueue.Dequeue();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    MyQueue.Enqueue(child);
                }
            }
            return null;
        }

        private void Scrubber_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var scrubber = sender as Slider;

            if (scrubber.Value != 50)
            {
                if (videoPlayer.CurrentState == MediaElementState.Playing)
                    ToggleVideoPlayPause();

                ClearLines();
                videoPlayer.Position = lastVideoPosition.Add(new TimeSpan(0, 0, 0, 0, (int)(scrubber.Value - 50) * 75));
            }
        }

        private async void OpenFile_HyperlinkButton_Handler(object sender, TappedRoutedEventArgs path)
        {
            var link = sender as HyperlinkButton;

            if (link == null)
                return;

            try
            {
                string filePath = ((FileModel)link.DataContext).Path;
                var fileInfo = LocalIO.OpenFile(filePath, false);

                HideAppBars();
                pnlControls.Visibility = Visibility.Visible;
                videoPlayer.SetSource(fileInfo.Stream, fileInfo.ContentType);
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("There was an error opening the specified file.\r\n\r\n" + ex.Message, "Error opening file");
                await dialog.ShowAsync();
                return;
            }
        }

        private void ClearLines(bool restoreSave = false, bool restoreTemplate = false)
        {
            LineCanvas.Children.Clear();

            if (restoreSave)
                foreach (var line in _savePoint)
                    LineCanvas.Children.Add(line);

            if (restoreTemplate)
                foreach (var line in _activeTemplate)
                    LineCanvas.Children.Add(line);
        }
        


        private void HideRinks()
        {
            HalfRink.Visibility = Visibility.Collapsed;
            FullRink.Visibility = Visibility.Collapsed;
            Scrubber.Visibility = Visibility.Visible;
        }

        private void HideAppBars()
        {
            TopBar.IsOpen = false;
            BottomBar.IsOpen = false;
        }

        private void BtnHalfIce_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ClearLines(false, false);
            HideRinks();
            HideAppBars();
            videoPlayer.Visibility = Visibility.Collapsed;
            HalfRink.Visibility = Visibility.Visible;
            Scrubber.Visibility = Visibility.Collapsed;
        }

        private void BtnFullIce_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ClearLines();
            HideRinks();
            HideAppBars();
            Scrubber.Visibility = Visibility.Collapsed;
            videoPlayer.Visibility = Visibility.Collapsed;
            FullRink.Visibility = Visibility.Visible;
        }

        private void BtnMoreControls_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlMoreControls.Visibility = Visibility.Visible;
        }

        private void BtnClear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_lines.Count == _savePoint.Count
                || _lines.Count == _activeTemplate.Count
                || _lines.Count == _savePoint.Count + _activeTemplate.Count)
            {
                _savePoint.Clear();
                _activeTemplate.Clear();
            }

            ClearLines(true, true);
        }

        private void BtnPlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleVideoPlayPause();
        }

        private void ToggleVideoPlayPause()
        {
            ClearLines();

            switch (videoPlayer.CurrentState)
            {
                case MediaElementState.Stopped:
                case MediaElementState.Playing:
                    videoPlayer.Pause();
                    btnPlay.Icon = new SymbolIcon(Symbol.Play);
                    btnPlay.Label = "Play";
                    lastVideoPosition = videoPlayer.Position;
                    break;
                case MediaElementState.Paused:
                    HideRinks();
                    HideAppBars();
                    videoPlayer.Visibility = Visibility.Visible;
                    videoPlayer.Play();
                    Scrubber.Value = 50;
                    btnPlay.Icon = new SymbolIcon(Symbol.Pause);
                    btnPlay.Label = "Pause";
                    break;
            }
        }

        private void HideAppBar()
        {
            pnlControls.Visibility = Visibility.Collapsed;
        }

        private void BtnOpen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalIO.SelectAndOpenFile();
        }

        private void ColorButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Color = ((SolidColorBrush)((AppBarButton)sender).Background).Color;
            UpdateInk(_inkDA);
        }

        private void HideOptionsBar()
        {
            HideAppBars();
        }

        private void BtnMore_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlMoreOptions.Visibility = Visibility.Visible;
        }

        private void btnSmall_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Size = new Size(5, 5);
            UpdateInk(_inkDA);
        }

        private void btnMed_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Size = new Size(10, 10);
            UpdateInk(_inkDA);
        }

        private void btnLarge_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Size = new Size(20, 20);
            UpdateInk(_inkDA);
        }

        private void UpdateInk(InkDrawingAttributes inkDA)
        {
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(inkDA);
            HideOptionsBar();
        }

        private void btnHighlight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var btn = sender as AppBarButton;

            _inkDA.DrawAsHighlighter = !_inkDA.DrawAsHighlighter;
            UpdateInk(_inkDA);

            if (_inkDA.DrawAsHighlighter)
                btn.Background = new SolidColorBrush(Color.FromArgb(44, 9, 00, 00));
            else
                btn.Background = new SolidColorBrush(Color.FromArgb(0, 9, 00, 00));

            HideOptionsBar();
        }

        private void BtnStraightLine_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _currentPen.IsFreehand = false;
        }

        private void BtnFreehand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _currentPen.IsFreehand = true;
        }

        private void BtnSolid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _currentPen.LineStyle = QuickPenModel.LineType.Solid;
        }
        
        private void BtnDashed_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _currentPen.LineStyle = QuickPenModel.LineType.Dashed;
        }

        private void BtnDouble_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _currentPen.LineStyle = QuickPenModel.LineType.Double;
        }
    }
}
