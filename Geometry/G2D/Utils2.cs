using System.Collections.Generic;
using System.Linq;

namespace Geometry.G2D
{
    public static class Utils2
    {
        public static Point2 ProjectionOnLine(Point2 p, Point2 p1, Point2 p2)
        {
            if (p1 == p2) return p1;
            var r1 = p - p1;
            var r2 = p2 - p1;
            var t = (r1.X*r2.X + r1.Y*r2.Y)/(r2.X*r2.X + r2.Y*r2.Y);
            return new Point2(p1.X + r2.X*t, p1.Y + r2.Y*t);
        }

        public static List<Point2> SortPointInLine(IList<Point2> points)
        {
            if (points.Count <= 1) return points.ToList();
            var origin = points[0];
            var v = (points[1] - points[0]).Normalize();
            var ts = points.Select(point3 => Vector2.Dot(point3 - origin, v)).ToList();
            ts.Sort((t1, t2) => t1 >= t2 ? 1 : -1);
            return ts.Select(t => origin + t*v).ToList();
        }

        public static Point2 GetLineIntersection(Point2 p, Vector2 v1, Point2 q, Vector2 v2)
        {
            var u = p - q;
            var t = Vector2.Cross(v2, u)/Vector2.Cross(v1, v2);
            return p + v1*t;
        }
    }
}
