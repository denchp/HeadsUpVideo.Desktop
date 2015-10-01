using HeadsUpVideo.Desktop.Interfaces;
using HeadsUpVideo.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace HeadsUpVideo.Desktop.CustomControls
{
    public class CustomInkCanvas : InkCanvas, ICustomCanvas
    {
        List<Path> _savePoint;
        List<Path> _activeTemplate;
        List<Path> _lines;
        List<Polyline> _polylines;
        List<Polyline> _polylineSavePoint;
        InkSynchronizer _synch;

        Canvas Canvas { get; set; }
        PenViewModel CurrentPen { get; set; }

        public CustomInkCanvas()
        {
            _synch = this.InkPresenter.ActivateCustomDrying();

            this.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
            this.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        public void Initialize(Canvas canvas, PenViewModel pen)
        {
            _savePoint = new List<Path>();
            _activeTemplate = new List<Path>();
            _lines = new List<Path>();
            _polylines = new List<Polyline>();
            _polylineSavePoint = new List<Polyline>();

            CurrentPen = pen;
            Canvas = canvas;

            CurrentPen.PropertyChanged += PenChanged;
            PenChanged(this, null);
        }

        private void PenChanged(object sender, PropertyChangedEventArgs e)
        {
            var newInk = new InkDrawingAttributes()
            {
                Color = CurrentPen.Color,
                FitToCurve = false,
                Size = new Size(CurrentPen.Size, CurrentPen.Size),
                IgnorePressure = true
            };

            this.InkPresenter.UpdateDefaultDrawingAttributes(newInk);
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
                    StrokeThickness = CurrentPen.Size,
                    StrokeEndLineCap = PenLineCap.Triangle,
                    Data = geometry
                };


                if (CurrentPen.EnableArrow)
                {
                    var arrowheadPath = new Polyline();
                    arrowheadPath.Fill = new SolidColorBrush(CurrentPen.Color);

                    foreach (var p in LineHelpers.DrawArrow(bearingPoint, lastPoint, CurrentPen.Size * 1.3))
                        arrowheadPath.Points.Add(p);

                    _polylines.Add(arrowheadPath);
                }


                if (CurrentPen.LineStyle == PenViewModel.LineType.Dashed)
                    path.StrokeDashArray = new DoubleCollection() { 5, 2 };

                _lines.Add(path);

                Canvas.Children.Clear();

                foreach (var line in _lines)
                    Canvas.Children.Add(line);

                foreach (var polyline in _polylines)
                    Canvas.Children.Add(polyline);

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
            Canvas.Children.Clear();
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
                Canvas.Children.Add(line);

            foreach (var polyline in _polylines)
                Canvas.Children.Add(polyline);
        }

    }
}
