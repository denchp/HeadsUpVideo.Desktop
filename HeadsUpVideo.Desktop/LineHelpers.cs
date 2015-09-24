using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace HeadsUpVideo.Desktop
{
    public static class LineHelpers
    {
        public static void GetCurveControlPoints(Point[] knots, out Point[] firstControlPoints, out Point[] secondControlPoints)
        {
            if (knots == null)
                throw new ArgumentNullException("knots");
            int n = knots.Length - 1;
            if (n < 1)
                throw new ArgumentException("At least two knot points required", "knots");
            if (n == 1)
            { // Special case: Bezier curve should be a straight line.
                firstControlPoints = new Point[1];
                // 3P1 = 2P0 + P3
                firstControlPoints[0].X = (2 * knots[0].X + knots[1].X) / 3;
                firstControlPoints[0].Y = (2 * knots[0].Y + knots[1].Y) / 3;

                secondControlPoints = new Point[1];
                // P2 = 2P1 – P0
                secondControlPoints[0].X = 2 * firstControlPoints[0].X - knots[0].X;
                secondControlPoints[0].Y = 2 * firstControlPoints[0].Y - knots[0].Y;
                return;
            }

            // Calculate first Bezier control points
            // Right hand side vector
            double[] rhs = new double[n];

            // Set right hand side X values
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].X + 2 * knots[i + 1].X;
            rhs[0] = knots[0].X + 2 * knots[1].X;
            rhs[n - 1] = (8 * knots[n - 1].X + knots[n].X) / 2.0;
            // Get first control points X-values
            double[] x = GetFirstControlPoints(rhs);

            // Set right hand side Y values
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].Y + 2 * knots[i + 1].Y;
            rhs[0] = knots[0].Y + 2 * knots[1].Y;
            rhs[n - 1] = (8 * knots[n - 1].Y + knots[n].Y) / 2.0;
            // Get first control points Y-values
            double[] y = GetFirstControlPoints(rhs);

            // Fill output arrays.
            firstControlPoints = new Point[n];
            secondControlPoints = new Point[n];
            for (int i = 0; i < n; ++i)
            {
                // First control point
                firstControlPoints[i] = new Point(x[i], y[i]);
                // Second control point
                if (i < n - 1)
                    secondControlPoints[i] = new Point(2 * knots[i + 1].X - x[i + 1], 2 * knots[i + 1].Y - y[i + 1]);
                else
                    secondControlPoints[i] = new Point((knots[n].X + x[n - 1]) / 2, (knots[n].Y + y[n - 1]) / 2);
            }
        }

        public static Point[] DrawArrow(Point p1, Point p2, double length)
        {

            var points = new List<Point>();
            Point p3 = new Point(p1.X, 0);
            double degree = Math.Atan2(p2.Y - p1.Y, p2.X- p1.X) - Math.Atan2(p3.Y - p1.Y, p3.X - p1.X);
            degree = degree * (180.0 / Math.PI); // Convert to degrees

            points.Add(RotatePoint(new Point(p2.X - length, p2.Y + length), p2, degree));
            points.Add(RotatePoint(new Point(p2.X + length, p2.Y + length), p2, degree));
            points.Add(RotatePoint(new Point(p2.X, p2.Y - length * 2.2), p2, degree));
            
            return points.ToArray();
            //var points = new List<Point>();

            //// Find the arrow shaft unit vector.
            //double vx = p2.X - p1.X;
            //double vy = p2.Y - p1.Y;
            //double dist = (float)Math.Sqrt(vx * vx + vy * vy);
            //vx /= dist;
            //vy /= dist;

            //points.AddRange(DrawArrowhead(p1, vx, vy, length));
            //return points.ToArray();

        }

        static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        private static List<Point> DrawArrowhead(Point p, double nx, double ny, double length)
        {
            var points = new List<Point>();

            double ax = length * (-ny - nx);
            double ay = length * (nx - ny);
            return new List<Point>()
            {
                new Point(p.X + ax, p.Y + ay), p, new Point(p.X - ay, p.Y + ax)
            };
        }

        private static double[] GetFirstControlPoints(double[] rhs)
        {
            int n = rhs.Length;
            double[] x = new double[n]; // Solution vector.
            double[] tmp = new double[n]; // Temp workspace.

            double b = 2.0;
            x[0] = rhs[0] / b;
            for (int i = 1; i < n; i++) // Decomposition and forward substitution.
            {
                tmp[i] = 1 / b;
                b = (i < n - 1 ? 4.0 : 3.5) - tmp[i];
                x[i] = (rhs[i] - x[i - 1]) / b;
            }
            for (int i = 1; i < n; i++)
                x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.

            return x;
        }
    }
}