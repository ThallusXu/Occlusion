using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Arithmetic;

namespace Geometry.G2D
{
    public static class Triangle 
    {
        public static bool IsTriangle(Point2 p1, Point2 p2, Point2 p3)
        {
            var l1 = (p1 - p2).Length;
            var l2 = (p1 - p3).Length;
            var l3 = (p2 - p3).Length;
            return l1 + l2 > l3 && l1 + l3 > l2 && l2 + l3 > l1;
        }

        public static Point2 FindCircumcenter(Point2 p1, Point2 p2, Point2 p3)
        {
            if (!IsTriangle(p1, p2, p3)) return null;
            var ax = p2.X - p1.X;
            var ay = p2.Y - p1.Y;
            var bx = p3.X - p1.X;
            var by = p3.Y - p1.Y;
            var d = 2*(ax*by - ay*bx);
            return new Point2(p1.X + (by*(ax*ax + ay*ay) - ay*(bx*bx + by*by))/d,
                              p1.Y + (ax*(bx*bx + by*by) - bx*(ax*ax + ay*ay))/d);
        }

        public static double SignedArea(Point2 p1, Point2 p2, Point2 p3)
        {
            return Vector2.Cross(p2 - p1, p3 - p1)*.5;
        }

        public static double Area(Point2 p1, Point2 p2, Point2 p3)
        {
            return Math.Abs(SignedArea(p1, p2, p3));
        }

        public static bool IsCounterClockwise(Point2 p1, Point2 p2, Point2 p3)
        {
            return SignedArea(p1, p2, p3) > 0;
        }
    }

    public class Polygon : IEnumerable<Point2>
    {
        private List<Point2> _points;

        public Polygon(IEnumerable<Point2> points)
        {
            _points = new List<Point2>(points);
        }

        private Polygon(int pointCount)
        {
            _points = new List<Point2>(pointCount);
        }

        public IEnumerator<Point2> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Point2 this[int i] => _points[i];

        public int PointCount => _points.Count;

        public double Area()
        {
            var res = 0.0;
            var p0 = this[0];
            for (var i = 1; i < PointCount; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1)%PointCount];
                res += Triangle.Area(p0, p1, p2);
            }
            return res;
        }

        public bool IsCounterClockwise()
        {
            var res = 0.0;
            var p0 = this[0];
            for (var i = 1; i < PointCount; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1)%PointCount];
                res += Triangle.Area(p0, p1, p2);
            }
            return res > 0;
        }

        // In-place
        public void MakeCounterClockwise()
        {
            if (IsCounterClockwise()) return;
            _points.Reverse(1, PointCount - 1);
        }

        // In-place
        public void MakeClockwise()
        {
            if (!IsCounterClockwise()) return;
            _points.Reverse(1, PointCount - 1);
        }

        public Polygon Rotate(double angle, Point2 center)
        {
            var res = new Polygon(PointCount)
            {
                _points = _points
                    .Select(p => center + (p - center).Rotate(angle)).ToList()
            };
            return res;
        }

        public static Polygon operator +(Polygon polygon, Vector2 v)
        {
            var res = new Polygon(polygon.PointCount){_points = polygon._points.Select(p => p + v).ToList()};
            return res;
        }

        public static Polygon operator -(Polygon polygon, Vector2 v)
        {
            return polygon + -v;
        }

        // Cite from ClipperLib
        // returns 0 if false, +1 if true, -1 if pt ON polygon boundary
        // See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
        // http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
        public int IsPointInPolygon(Point2 p)
        {
            var res = 0;
            var n = PointCount;
            if (n < 3) return 0;
            var ip = this[0];
            for (var i = 1; i <= n; i++)
            {
                var ipNext = this[i%n];
                if (ipNext.Y.Near(p.Y))
                {
                    if (ipNext.X.Near(p.X) ||
                        (ip.Y.Near(p.Y) && ipNext.X > p.X == ip.X < p.X))
                        return -1;
                }
                if (ip.Y < p.Y != ipNext.Y < p.Y)
                {
                    if (ip.X >= p.X)
                    {
                        if (ipNext.X > p.X) res = 1 - res;
                        else
                        {
                            var d = (ip.X - p.X)*(ipNext.Y - p.Y) -
                                    (ipNext.X - p.X)*(ip.Y - p.Y);
                            if (d.Near(0)) return -1;
                            if (d > 0 == ipNext.Y > ip.Y) res = 1 - res;
                        }
                    }
                    else
                    {
                        if (ipNext.X > p.X)
                        {
                            var d = (ip.X - p.X)*(ipNext.Y - p.Y) -
                                    (ipNext.X - p.X)*(ip.Y - p.Y);
                            if (d.Near(0)) return -1;
                            if (d > 0 == ipNext.Y > ip.Y) res = 1 - res;
                        }
                    }
                }
                ip = ipNext;
            }
            return res;
        }

        public override string ToString()
        {
            return $"Polygon({string.Join(", ", _points)})";
        }
    }

    public class PolygonWithHole
    {
        public Polygon Outer { get; }
        public List<Polygon> Inners { get; }

        public PolygonWithHole(Polygon outer, List<Polygon> inners)
        {
            Outer = outer;
            Inners = inners;
        }

        public PolygonWithHole(Polygon outer, Polygon inner)
        {
            Outer = outer;
            Inners = new List<Polygon> {inner};
        }

        public static PolygonWithHole operator +(PolygonWithHole pwh, Vector2 v)
        {
            return new PolygonWithHole(pwh.Outer + v, pwh.Inners.Select(polygon => polygon + v).ToList());
        }

        public static PolygonWithHole operator -(PolygonWithHole pwh, Vector2 v)
        {
            return pwh + -v;
        }

        public PolygonWithHole Rotate(double angle, Point2 center)
        {
            return new PolygonWithHole(Outer.Rotate(angle, center), Inners.Select(polygon => polygon.Rotate(angle, center)).ToList());
        }
    }
}
