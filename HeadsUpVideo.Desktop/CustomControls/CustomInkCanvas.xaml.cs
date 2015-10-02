using HeadsUpVideo.Desktop.Interfaces;
using HeadsUpVideo.Desktop.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
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
        PenViewModel _currentPen;

        public CustomInkCanvas()
        {
            this.InitializeComponent();
            _inkCanvas = this.InkCanvas;
            _canvas = this.LineCanvas;

            _synch = _inkCanvas.InkPresenter.ActivateCustomDrying();

            _inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
            _inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        public void Initialize(PenViewModel pen)
        {
            _savePoint = new List<Path>();
            _activeTemplate = new List<Path>();
            _lines = new List<Path>();
            _polylines = new List<Polyline>();
            _polylineSavePoint = new List<Polyline>();

            _currentPen = pen;
            _currentPen.PropertyChanged += PenChanged;

            PenChanged(this, null);
        }

        private void PenChanged(object sender, PropertyChangedEventArgs e)
        {
            var newInk = new InkDrawingAttributes()
            {
                Color = _currentPen.Color,
                FitToCurve = false,
                Size = new Size(_currentPen.Size, _currentPen.Size),
                IgnorePressure = false,
                DrawAsHighlighter = _currentPen.IsHighlighter
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
                    Stroke = new SolidColorBrush(_currentPen.Color),
                    Opacity = _currentPen.IsHighlighter ? .5 : 1,
                    StrokeThickness = _currentPen.Size,
                    StrokeEndLineCap = PenLineCap.Triangle,
                    Data = geometry
                };


                if (_currentPen.EnableArrow)
                {
                    var arrowheadPath = new Polyline()
                    {
                        Fill = new SolidColorBrush(_currentPen.Color),
                        Opacity = _currentPen.IsHighlighter ? .5 : 1
                    };

                    foreach (var p in LineHelpers.DrawArrow(bearingPoint, lastPoint, _currentPen.Size * 1.3))
                        arrowheadPath.Points.Add(p);

                    _polylines.Add(arrowheadPath);
                }


                if (_currentPen.LineStyle == PenViewModel.LineType.Dashed)
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
            _currentPen = currentPen;
            PenChanged(this, null);
        }
    }
}