using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Geometry.Arithmetic;
using Geometry.G2D;

namespace Geometry.G3D
{
    public class SimpleSurface : ITransformational<SimpleSurface>
    {
        private List<DirectedSegment3> _outer;
        private List<List<DirectedSegment3>> _inners;

        public IReadOnlyList<DirectedSegment3> Outer => _outer;
        public IReadOnlyList<IReadOnlyList<DirectedSegment3>> Inners => _inners;

        private SimpleSurface() {}

        public SimpleSurface(IReadOnlyList<Point3> outer)
        {
            var n = outer.Count;
            if (n < 3) throw new GeometryException("failed to generate a SimpleSurface with less than 3 points");
            _outer = new List<DirectedSegment3>();
            _inners = new List<List<DirectedSegment3>>();
            for (var i = 0; i < n; i++) _outer.Add(new DirectedSegment3(outer[i], outer[(i + 1)%n]));
        }

        public void AddInner(IList<Point3> points)
        {
            var n = points.Count;
            if (n < 3) throw new GeometryException("failed to add a whole to a SimpleSurface with less than 3 points.");
            var inner = new List<DirectedSegment3>();
            for (var i = 0; i < n; i++) inner.Add(new DirectedSegment3(points[i], points[(i + 1)%n]));
            _inners.Add(inner);
        }

        public Vector3 Normal
        {
            get
            {
                var v1 = _outer.First().P2 - _outer.First().P1;
                var v2 = _outer.Last().P2 - _outer.Last().P1;
                return Vector3.Cross(v2, v1).Normalize();
            }
        }

        public SimpleSurface Rotate(Quaternion quaternion)
        {
            return new SimpleSurface
            {
                _outer = _outer.Select(edge => edge.Rotate(quaternion)).ToList(),
                _inners = _inners
                    .Select(inner => inner
                                .Select(edge => edge.Rotate(quaternion)).ToList())
                    .ToList()
            };
        }

        public SimpleSurface Move(Vector3 v, double distance)
        {
            return new SimpleSurface
            {
                _outer = _outer.Select(edge => edge.Move(v, distance)).ToList(),
                _inners = _inners
                    .Select(inner => inner
                                .Select(edge => edge.Move(v, distance)).ToList())
                    .ToList()
            };
        }

        public SimpleSurface Move(Vector3 v)
        {
            return new SimpleSurface
            {
                _outer = _outer.Select(edge => edge.Move(v)).ToList(),
                _inners = _inners
                    .Select(inner => inner
                                .Select(edge => edge.Move(v)).ToList())
                    .ToList()
            };
        }

        public CoordinateSystem3 GetCoordinate()
        {
            var axisX = _outer.First().Direction;
            var axisZ = Normal.Normalize();
            var axisY = Vector3.Cross(axisZ, axisX);
            var origin = _outer.First().P1;
            return new CoordinateSystem3(origin, axisX, axisY, axisZ);
        }

        public static Point2 ToPointInSpecificPlane(Point3 p, Vector3 axisX, Vector3 axisY, Point3 origin)
        {
            axisX = axisX.Normalize();
            axisY = axisY.Normalize();
            return new Point2(Vector3.Dot(p - origin, axisX), Vector3.Dot(p - origin, axisY));
        }

        public static bool GetPlaneCoordinate(SimpleSurface face, out Vector3 axisX, out Vector3 axisY, out Point3 origin)
        {
            axisX = face.Outer.First().Direction;
            axisY = Vector3.Cross(face.Normal, axisX);
            origin = face.Outer.First().P1;
            return true;
        }

        public static bool IsPointInSurface2(Point3 p, SimpleSurface f, bool includingBorder = false)
        {
            if (!Vector3.Dot(p - f.Outer.First().P1, f.Normal).Near(0)) return false;
            Vector3 axisX, axisY;
            Point3 origin;
            var gotCoord = GetPlaneCoordinate(f, out axisX, out axisY, out origin);
            var polygon = new Polygon(f.Outer.Select(edge => ToPointInSpecificPlane(edge.P1, axisX, axisY, origin)));
            var p2D = ToPointInSpecificPlane(p, axisX, axisY, origin);
            if (polygon.IsPointInPolygon(p2D) == 1)
            {
                foreach (var inner in f.Inners)
                {
                    var innerPoly = new Polygon(inner.Select(edge => ToPointInSpecificPlane(edge.P1, axisX, axisY, origin)));
                    if (innerPoly.IsPointInPolygon(p2D) == 1 || (includingBorder && innerPoly.IsPointInPolygon(p2D) == -1)) return false;
                }
                return true;
            }
            return includingBorder && polygon.IsPointInPolygon(p2D) == -1;
        }

        public bool IsPointInSurface(Point3 p, bool includingBorder = false)
        {
            if (!Vector3.Dot(p - _outer.First().P1, Normal).Near(0)) return false;
            var coordinate = GetCoordinate();
            var polygon = new Polygon(_outer.Select(edge => coordinate.ToLocal(edge.P1).ToPoint2(EPlane.Xoy)));
            var p2D = coordinate.ToLocal(p).ToPoint2(EPlane.Xoy);
            if (polygon.IsPointInPolygon(p2D) == 1)
            {
                return _inners
                    .Select(inner => new Polygon(inner
                                                     .Select(edge => coordinate.ToLocal(edge.P1).ToPoint2(EPlane.Xoy))))
                    .All(innerPoly =>
                            innerPoly.IsPointInPolygon(p2D) != 1 &&
                            (!includingBorder || innerPoly.IsPointInPolygon(p2D) != -1));
            }
            return includingBorder && polygon.IsPointInPolygon(p2D) == -1;
        }

        public override string ToString()
        {
            var s = new StringBuilder("SimpleSurface:{\n Outer:\n");
            foreach (var edge in _outer)
            {
                s.Append($"\t{edge}\n");
            }
            s.Append("Inners:\n");
            foreach (var inner in _inners)
            {
                s.Append("\tInner:\n");
                foreach (var edge in inner)
                {
                    s.Append($"\t\t{edge}\n");
                }
            }
            return s.ToString();
        }
    }

    public interface ISurface : ITransformational<ISurface>
    {
        List<SimpleSurface> DecomposeToSimpleSurfaces();
        List<DirectedSegment3> VisibleSegments();
    }

    public class FlatPlane : ISurface
    {
        private List<IEdge> _outer;
        private List<List<IEdge>> _inners;

        public IReadOnlyList<IEdge> Outer => _outer;
        public IReadOnlyList<IReadOnlyList<IEdge>> Inners => _inners;

        public FlatPlane(List<List<Point3>> points)
        {
            _outer = new List<IEdge>();
            _inners = new List<List<IEdge>>();
            foreach (var ps in points)
            {
                Debug.Assert(ps.Count >= 2);
#if !NO_EXCEPTION
                if (ps.Count < 2)
                    throw new GeometryException("could not construct an Edge of a surface with less than 2 points");
#endif
                if (ps.Count == 2) _outer.Add(new StraightEdge(ps[0], ps[1]));
                else _outer.Add(new CurveEdge(ps));
            }
        }

        private FlatPlane()
        {
            _outer = new List<IEdge>();
            _inners = new List<List<IEdge>>();
        }

        public Vector3 Normal => Vector3.Cross(_outer[0].Decompose()[0].Direction, _outer[1].Decompose()[0].Direction);

        public void AddInner(IList<IList<Point3>> points)
        {
            var inner = new List<IEdge>();
            foreach (var ps in points)
            {
                Debug.Assert(ps.Count >= 2);
#if !NO_EXCEPTION
                if (ps.Count < 2)
                    throw new GeometryException("could not construct an Edge of a surface with less than 2 points");
#endif
                if (ps.Count == 2) inner.Add(new StraightEdge(ps[0], ps[1]));
                else inner.Add(new CurveEdge(ps));
            }
            _inners.Add(inner);
        }

        public ISurface Rotate(Quaternion quaternion)
        {
            return new FlatPlane
            {
                _outer = _outer.Select(edge => edge.Rotate(quaternion)).ToList(),
                _inners = _inners
                    .Select(inner => inner.Select(edge => edge.Rotate(quaternion)).ToList())
                    .ToList()
            };
        }

        public ISurface Move(Vector3 v, double distance)
        {
            return new FlatPlane
            {
                _outer = _outer.Select(edge => edge.Move(v, distance)).ToList(),
                _inners = _inners
                    .Select(inner => inner.Select(edge => edge.Move(v, distance)).ToList())
                    .ToList()
            };
        }

        public ISurface Move(Vector3 v)
        {
            var res = new FlatPlane
            {
                _outer = _outer.Select(edge => edge.Move(v)).ToList(),
                _inners = _inners
                    .Select(inner => inner.Select(edge => edge.Move(v)).ToList())
                    .ToList()
            };
            return res;
        }

        public List<SimpleSurface> DecomposeToSimpleSurfaces()
        {
            var ps = _outer.SelectMany(edge => edge.Decompose().Select(s => s.P1)).ToList();
            var res = new SimpleSurface(ps);
            foreach (var inner in _inners)
            {
                ps = inner.SelectMany(edge => edge.Decompose().Select(s => s.P1)).ToList();
                res.AddInner(ps);
            }
            return new List<SimpleSurface> {res};
        }

        public List<DirectedSegment3> VisibleSegments()
        {
            var res = new List<DirectedSegment3>();
            res.AddRange(_outer.SelectMany(edge => edge.Decompose()));
            foreach (var inner in _inners)
            {
                res.AddRange(inner.SelectMany(edge => edge.Decompose()));
            }
            return res;
        }

        public void Reverse()
        {
            foreach (var edge in _outer) edge.Reverse();
            _outer.Reverse();
            foreach (var inner in _inners)
            {
                foreach (var edge in inner) edge.Reverse();
                inner.Reverse();
            }
            _inners.Reverse();
        }
    }

    public class RuledSurface : ISurface
    {
        private IEdge _top;
        private IEdge _bottom;

        private RuledSurface() {}

        public RuledSurface(IEdge top, IEdge bottom)
        {
            _top = top;
            _bottom = bottom;
        }

        public RuledSurface(IEdge edge, Vector3 backward, Vector3 forward)
        {
            _top = edge.Move(forward);
            _bottom = edge.Move(backward);
        }

        public ISurface Rotate(Quaternion quaternion)
        {
            return new RuledSurface
            {
                _top = _top.Rotate(quaternion),
                _bottom = _bottom.Rotate(quaternion)
            };
        }

        public ISurface Move(Vector3 v, double distance)
        {
            return new RuledSurface
            {
                _top = _top.Move(v, distance),
                _bottom = _bottom.Move(v, distance)
            };
        }

        public ISurface Move(Vector3 v)
        {
            return new RuledSurface
            {
                _top = _top.Move(v),
                _bottom = _bottom.Move(v)
            };
        }

        public List<SimpleSurface> DecomposeToSimpleSurfaces()
        {
            var topSeg = _top.Decompose();
            var botSeg = _bottom.Decompose();
            return topSeg
                .Select((t, i) => new SimpleSurface(new List<Point3>
                {
                    t.P1,
                    t.P2,
                    botSeg[i].P2,
                    botSeg[i].P1
                })).ToList();
        }

        private List<DirectedSegment3> Beams()
        {
            return _top.DecomposeToPoints()
                .Zip(_bottom.DecomposeToPoints(),
                     (f, b) => new DirectedSegment3(f, b)).ToList();
        }

        public List<DirectedSegment3> VisibleSegments()
        {
            var res = new List<DirectedSegment3>();
            var segs = Beams();
            if (_bottom.GetType() == typeof(CurveEdge) && _top.GetType() == typeof(CurveEdge))
            {
                var faces = DecomposeToSimpleSurfaces();
                var lastVisible = true;
                for (var i = 0; i < segs.Count; i++)
                {
                    var seg = segs[i];
                    foreach (var visible in from face
                                                in
                                                faces.GetRange(i - 1 >= 0 ? i - 1 : 0,
                                                               i - 1 < 0 || i + 1 > faces.Count ? 1 : 2)
                                            where face.IsPointInSurface(seg.Center, true)
                                            select Vector3.Dot(face.Normal, -Vector3.Y_AXIS) < 0)
                    {
                        if (visible != lastVisible) res.Add(seg);
                        lastVisible = visible;
                    }
                }
            }
            res.Add(segs.First());
            res.Add(segs.Last());
            return res;
        }
    }
}
