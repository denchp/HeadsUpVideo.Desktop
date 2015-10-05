using HeadsUpVideo.Desktop.Base;
using HeadsUpVideo.Desktop.Interfaces;
using HeadsUpVideo.Desktop.Models;
using HeadsUpVideo.Desktop.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HeadsUpVideo.Desktop.CustomControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomInkCanvas : UserControl, ICustomCanvas
    {
        List<Path> _savePoint;
        List<Path> _activeTemplate;
        List<Path> _lines;
        List<Polyline> _polylines;
        List<Polyline> _polylineSavePoint;
        InkSynchronizer _synch;
        InkCanvas _inkCanvas;
        Canvas _canvas;

        public Command ClearQuickPensCmd { get; set; }
        public Command SaveQuickPenCmd { get; set; }
        public Command ClearLinesCmd { get; set; }
        public Command CreateSavePointCmd { get; set; }
        public Command<string> SetLineStyleCmd { get; set; }
        public Command<PenViewModel> LoadQuickPenCmd { get; set; }

        public ObservableCollection<PenViewModel> QuickPens { get; set; }
        public PenViewModel CurrentPen { get; set; }


        public CustomInkCanvas()
        {
            CurrentPen = new PenViewModel()
            {
                IsFreehand = true,
                Color = Colors.Blue,
                LineStyle = PenViewModel.LineType.Solid,
                Size = 10
            };

            this.InitializeComponent();
            _inkCanvas = this.InkCanvas;
            _canvas = this.LineCanvas;
            _lines = new List<Path>();
            _polylines = new List<Polyline>();
            _savePoint = new List<Path>();
            _polylineSavePoint = new List<Polyline>();

            _synch = _inkCanvas.InkPresenter.ActivateCustomDrying();
            _inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
            _inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;

            QuickPens = new ObservableCollection<PenViewModel>();
            CreateSavePointCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = CreateSavePoint };
            ClearQuickPensCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = ClearQuickPens };
            SaveQuickPenCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = SaveQuickPen };
            LoadQuickPenCmd = new Command<PenViewModel>() { CanExecuteFunc = obj => true, ExecuteFunc = LoadQuickPen };
            ClearLinesCmd = new Command() { CanExecuteFunc = obj => true, ExecuteFunc = Clear };
            SetLineStyleCmd = new Command<string>() { CanExecuteFunc = obj => true, ExecuteFunc = SetLineStyle };
        }

        public void Initialize(PenViewModel pen)
        {
            _savePoint = new List<Path>();
            _activeTemplate = new List<Path>();
            _lines = new List<Path>();
            _polylines = new List<Polyline>();
            _polylineSavePoint = new List<Polyline>();

            CurrentPen = pen;
            CurrentPen.PropertyChanged += PenChanged;

            RefreshQuickPens();

            PenChanged(this, null);
        }

        private void PenChanged(object sender, PropertyChangedEventArgs e)
        {
            var newInk = new InkDrawingAttributes()
            {
                Color = CurrentPen.Color,
                FitToCurve = false,
                Size = new Size(CurrentPen.Size, CurrentPen.Size),
                IgnorePressure = false,
                DrawAsHighlighter = CurrentPen.IsHighlighter
            };

            _inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(newInk);
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

                if (!CurrentPen.IsFreehand)
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
                    lastPoint = points[i + 1];
                    bearingPoint = points[i];
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
                    Stroke = new SolidColorBrush(CurrentPen.Color),
                    Opacity = CurrentPen.IsHighlighter ? .5 : 1,
                    StrokeThickness = CurrentPen.Size,
                    StrokeEndLineCap = PenLineCap.Triangle,
                    Data = geometry
                };


                if (CurrentPen.EnableArrow)
                {
                    var arrowheadPath = new Polyline()
                    {
                        Fill = new SolidColorBrush(CurrentPen.Color),
                        Opacity = CurrentPen.IsHighlighter ? .5 : 1
                    };

                    foreach (var p in LineHelpers.DrawArrow(bearingPoint, lastPoint, CurrentPen.Size * 1.3))
                        arrowheadPath.Points.Add(p);

                    _polylines.Add(arrowheadPath);
                }


                if (CurrentPen.LineStyle == PenViewModel.LineType.Dashed)
                    path.StrokeDashArray = new DoubleCollection() { 5, 2 };

                _lines.Add(path);

                _canvas.Children.Clear();

                foreach (var line in _lines)
                    _canvas.Children.Add(line);

                foreach (var polyline in _polylines)
                    _canvas.Children.Add(polyline);
            }
        }

        public void CreateSavePoint()
        {
            _savePoint = new List<Path>();
            _polylineSavePoint = new List<Polyline>();

            foreach (var line in _lines)
                _savePoint.Add(line);

            foreach (var poly in _polylines)
                _polylineSavePoint.Add(poly);
        }

        public void Clear()
        {
            if (_lines.Count == _savePoint.Count
                || _lines.Count == _activeTemplate.Count
                || _lines.Count == _savePoint.Count + _activeTemplate.Count)
            {
                _savePoint.Clear();
                _polylineSavePoint.Clear();
                _activeTemplate.Clear();
            }

            ClearLines(true, true);
        }

        public void ClearLines(bool restoreSave = false, bool restoreTemplate = false)
        {
            _canvas.Children.Clear();
            _lines.Clear();
            _polylines.Clear();

            if (restoreSave)
            {
                foreach (var line in _savePoint)
                    _lines.Add(line);

                foreach (var polyline in _polylineSavePoint)
                    _polylines.Add(polyline);
            }

            if (restoreTemplate)
                foreach (var line in _activeTemplate)
                    _lines.Add(line);

            foreach (var line in _lines)
                _canvas.Children.Add(line);

            foreach (var polyline in _polylines)
                _canvas.Children.Add(polyline);
        }

        public void SetPen(PenViewModel currentPen)
        {
            CurrentPen = currentPen;
            PenChanged(this, null);
        }

        private void LoadQuickPen(PenViewModel obj)
        {
            CurrentPen = obj;
            SetPen(CurrentPen);
        }

        private void ClearQuickPens()
        {
            LocalIO.SaveQuickPens();
            RefreshQuickPens();
        }

        private void SaveQuickPen()
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

            LocalIO.SaveQuickPens(QuickPens);
        }

        private void QuickPens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            QuickPenButtons.Children.Clear();

            foreach (PenViewModel pen in QuickPens)
            {
                QuickPenButtons.Children.Add(new PenButton(pen)
                {
                    Command = LoadQuickPenCmd,
                    CommandParameter = pen
                });
            }
        }

        public void SetLineStyle(string style)
        {
            switch (style)
            {
                case "Solid":
                    CurrentPen.LineStyle = PenModel.LineType.Solid;
                    break;
                case "Dash":
                    CurrentPen.LineStyle = PenModel.LineType.Dashed;
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