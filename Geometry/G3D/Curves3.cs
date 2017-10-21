using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Geometry.Arithmetic;
using Geometry.G2D;

namespace Geometry.G3D
{
    public class DirectedSegment3 : ITransformational <DirectedSegment3>
    {
        public readonly Point3 P1, P2;

        public DirectedSegment3(Point3 p1, Point3 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public double Length => (P1 - P2).Length;

        public Vector3 Direction => (P2 - P1).Normalize();

        public Point3 Center => P1 + (P2 - P1)/2;

        public static DirectedSegment3 operator +(DirectedSegment3 s, Vector3 v)
        {
            return new DirectedSegment3(s.P1 + v, s.P2 + v);
        }

        public static DirectedSegment3 operator -(DirectedSegment3 s, Vector3 v)
        {
            return new DirectedSegment3(s.P1 - v, s.P2 - v);
        }

        public DirectedSegment3 Move(Vector3 v, double distance)
        {
            return new DirectedSegment3(P1.Move(v, distance), P2.Move(v, distance));
        }

        public DirectedSegment3 Move(Vector3 v)
        {
            return new DirectedSegment3(P1.Move(v), P2.Move(v));
        }

        public DirectedSegment3 Rotate(Quaternion quaternion)
        {
            return new DirectedSegment3(P1.Rotate(quaternion), P2.Rotate(quaternion));
        }

        public bool ContainsPoint(Point3 p, bool includingEndPoint = true)
        {
            if (p == P1 || p == P2) return includingEndPoint;
            return (p.DistanceTo(P1) + p.DistanceTo(P2) - Length).Near(0);
        }

        public DirectedSegment2 ToSegment2(EPlane plane)
        {
            return new DirectedSegment2(P1.ToPoint2(plane), P2.ToPoint2(plane));
        }

        public override string ToString()
        {
            return $"DirectedSegment3({P1}->{P2})";
        }
    }

    public interface IEdge : ITransformational<IEdge>
    {
        List<DirectedSegment3> Decompose();
        IReadOnlyList<Point3> DecomposeToPoints();
        void Reverse();
        Point3 First();
        Point3 Last();
    }

    public class StraightEdge : IEdge
    {
        public Point3 P1 { get; private set; }
        public Point3 P2 { get; private set; }

        public StraightEdge(Point3 p1, Point3 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public IEdge Rotate(Quaternion quaternion)
        {
            return new StraightEdge(P1.Rotate(quaternion), P2.Rotate(quaternion));
        }

        public IEdge Move(Vector3 v, double distance)
        {
            return new StraightEdge(P1.Move(v, distance), P2.Move(v, distance));
        }

        public IEdge Move(Vector3 v)
        {
            return new StraightEdge(P1.Move(v), P2.Move(v));
        }

        public List<DirectedSegment3> Decompose()
        {
            return new List<DirectedSegment3> {new DirectedSegment3(P1, P2)};
        }

        public IReadOnlyList<Point3> DecomposeToPoints()
        {
            return new List<Point3> {P1, P2};
        }

        public void Reverse()
        {
            var t = P1;
            P1 = P2;
            P2 = t;
        }

        public Point3 First()
        {
            return P1;
        }

        public Point3 Last()
        {
            return P2;
        }
    }

    public class CurveEdge : IEdge, IEnumerable<Point3>
    {
        private readonly List<Point3> _points;

        public CurveEdge(IList<Point3> points)
        {
            Debug.Assert(points != null && points.Count >= 3);
#if !NO_EXCEPTION
            if (points.Count < 3) throw new GeometryException("could not construct a CurveEdge with less than 3 points");
#endif
            _points = new List<Point3>();
            _points.AddRange(points);
        }

        public List<DirectedSegment3> Decompose()
        {
            var res = new List<DirectedSegment3>();
            for (var i = 0; i < _points.Count - 1; i++)
            {
                res.Add(new DirectedSegment3(_points[i], _points[i + 1]));
            }
            return res;
        }

        public IReadOnlyList<Point3> DecomposeToPoints()
        {
            return _points;
        }

        public void Reverse()
        {
            _points.Reverse();
        }

        public Point3 First()
        {
            return _points.First();
        }

        public Point3 Last()
        {
            return _points.Last();
        }

        public IEdge Rotate(Quaternion quaternion)
        {
            return new CurveEdge(_points.Select(p => p.Rotate(quaternion)).ToList());
        }

        public IEdge Move(Vector3 v, double distance)
        {
            return new CurveEdge(_points.Select(p => p.Move(v, distance)).ToList());
        }

        public IEdge Move(Vector3 v)
        {
            return new CurveEdge(_points.Select(p => p.Move(v)).ToList());
        }

        public IEnumerator<Point3> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _points.GetEnumerator();
        }
    }
}
