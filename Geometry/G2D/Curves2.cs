using System;
using System.Collections.Generic;
using Geometry.Arithmetic;

namespace Geometry.G2D
{
    public interface ICurve
    {
        List<Point2> Sample(int quality = 16);
    }

    public class DirectedSegment2 : ICurve
    {
        public readonly Point2 P1, P2;

        public DirectedSegment2(Point2 p1, Point2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        private DirectedSegment2(DirectedSegment2 ds)
        {
            P1 = ds.P1;
            P2 = ds.P2;
        }

        public double Length => (P1 - P2).Length;

        public Point2 Center => P1 + (P1 - P2)/2;

        public Vector2 Direction => (P2 - P1).Normalize();

        public Vector2 OrthogonalVector => (P2 - P1).Orthogonalize();

        public double Angle => (P2 - P1).ToPolarAngle();

        public bool Contains(Point2 p, double eps = Constants.DEFAULT_EPS)
        {
            var d = p.DistanceTo(this);
            return p.DistanceTo(this).Near(0, eps);
        }

        public static bool IsSegmentsProperIntersection(Point2 a1, Point2 a2, Point2 b1, Point2 b2)
        {
            double c1 = Vector2.Cross(a2 - a1, b1 - a1),
                c2 = Vector2.Cross(a2 - a1, b2 - a1),
                c3 = Vector2.Cross(b2 - b1, a1 - b1),
                c4 = Vector2.Cross(b2 - b1, a2 - b1);
            return c1.DCompareTo(0)*c2.DCompareTo(0) < 0 && c3.CompareTo(0)*c4.DCompareTo(0) < 0;
        }

        public static bool IsSegmentsProperIntersection(DirectedSegment2 s1, DirectedSegment2 s2)
        {
            return IsSegmentsProperIntersection(s1.P1, s1.P2, s2.P1, s2.P2);
        }

        // Sample quality is useless for segment.
        public List<Point2> Sample(int quality = 16)
        {
            return new List<Point2> {P1, P2};
        }

        public DirectedSegment2 Clone()
        {
            return new DirectedSegment2(this);
        }

        public override string ToString()
        {
            return $"DSegment({P1}, {P2})";
        }
    }

    public class Arc : ICurve
    {
        public readonly Point2 Head, Body, Tail;

        public Arc(Point2 head, Point2 body, Point2 tail)
        {
            Head = head;
            Body = body;
            Tail = tail;
        }

        public List<Point2> Sample(int quality = 64)
        {
            var center = Triangle.FindCircumcenter(Head, Body, Tail);
            var angle1 = Math.Atan2(Head.Y - center.Y, Head.X - center.X);
            var angle2 = Math.Atan2(Tail.Y - center.Y, Tail.X - center.X);
            var r = ((center - Head).Length + (center - Body).Length + (center - Tail).Length)/3;
            var angleDiff = Utils.NormalizeAngle(Math.Abs(angle1 - angle2));
            var segmentCount = (int) Math.Floor(angleDiff/Constants.PI*quality) + 1;
            
            var res = new List<Point2>();
            var each = angleDiff/segmentCount;
            for (var i = 0; i <= segmentCount; i++)
            {
                res.Add(center + Vector2.FromPolarAngle(angle1 + each*i)*r);
            }
            return res;
        }
    }

    public class Spline : ICurve
    {
        public List<Point2> Sample(int quality = 16)
        {
            throw new NotImplementedException();
        }
    }
}
