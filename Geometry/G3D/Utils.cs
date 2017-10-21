using System.Collections.Generic;
using System.Linq;
using Geometry.Arithmetic;

namespace Geometry.G3D
{
    public static class Utils
    {
        public static Point3 SegmentPlaneIntersection(Point3 pSeg, Vector3 vSeg, Point3 pPlane, Vector3 vPlane)
        {
            var deno = vPlane.X*vSeg.X + vPlane.Y*vSeg.Y + vPlane.Z*vSeg.Z;
            if (deno.Near(0)) return null;
            var v = pPlane - pSeg;
            var t = (vPlane.X*v.X + vPlane.Y*v.Y + vPlane.Z*v.Z)/deno;
            return new Point3(pSeg.X + vSeg.X*t, pSeg.Y + vSeg.Y*t, pSeg.Z + vSeg.Z*t);
        }

        public static bool IsPointOccluded(Point3 point, SimpleSurface face, Vector3 direction, double threshold = Constants.DEFAULT_EPS)
        {
            direction = direction.Normalize();
            var inter = SegmentPlaneIntersection(point, direction, face.Outer.First().P1, face.Normal);
            if (inter == null || Vector3.Dot(inter - point, direction) < threshold) return false;
            return face.IsPointInSurface(inter, true);
        }

        public static List<Point3> SortInLine3(IList<Point3> points)
        {
            if (points.Count <= 1) return points.ToList();
            var origin = points[0];
            var v = (points[1] - points[0]).Normalize();
            // transfer to parameter space
            var ts = points.Select(point3 => Vector3.Dot(point3 - origin, v)).ToList();
            ts.Sort();
            return ts.Select(t => origin + t*v).ToList();
        }

        public static bool IsLinesInTheSamePlane(Point3 p1, Vector3 v1, Point3 p2, Vector3 v2)
        {
            var v = p2 - p1;
            return Vector3.Cross(Vector3.Cross(v1, v), Vector3.Cross(v2, v)).Length.Near(0);
        }

        public static bool LineIntersection(Point3 p1, Vector3 v1, Point3 p2, Vector3 v2, out Point3 intersection)
        {
            intersection = null;
            if (!IsLinesInTheSamePlane(p1, v1, p2, v2)) return false;
            double t;
            if (!(v1.X*v2.Y - v1.Y*v2.X).Near(0)) t = (v1.Y*(p1.X - p2.X) + v1.X*(p2.Y - p1.Y))/(v2.X*v1.Y - v1.X*v2.Y);
            else if (!(v1.X*v2.Z - v1.Z*v2.X).Near(0)) t = (v1.Z*(p1.X - p2.X) + v1.X*(p2.Z - p1.Z))/(v2.X*v1.Z - v1.X*v2.Z);
            else if (!(v1.Y*v2.Z - v1.Z*v2.Y).Near(0)) t = (v1.Z*(p1.Y - p2.Y) + v1.Y*(p2.Z - p1.Z))/(v2.Y*v1.Z - v1.Y*v2.Z);
            else return false;
            intersection = p2 + v2*t;
            return true;
        }

        public static bool PlaneIntersection(
            Point3 p1, Vector3 normal1,
            Point3 p2, Vector3 normal2,
            out Point3 interp, out Vector3 interDir)
        {
            interp = null;
            interDir = null;
            var n = Vector3.Cross(normal1, normal2);
            if (n.Length.Near(0)) return false;
            interDir = n.Normalize();
            var s1 = Vector3.Dot(normal1, p1 - Point3.ZERO_POINT);
            var s2 = Vector3.Dot(normal2, p2 - Point3.ZERO_POINT);
            var dot = Vector3.Dot(normal1, normal2);
            var t = (s2*dot - s1)/(dot*dot - 1);
            var s = (s1*dot - s2)/(dot*dot - 1);
            interp = Point3.ZERO_POINT + normal1*t + normal2*s;
            return true;
        }

        public static List<DirectedSegment3> FaceIntersection(SimpleSurface f1, SimpleSurface f2, bool includingBorder = false)
        {
            var res = new List<DirectedSegment3>();
            Vector3 dir;
            Point3 p;
            var inter = PlaneIntersection(f1.Outer.First().P1, f1.Normal, f2.Outer.First().P1, f2.Normal, out p, out dir);
            if (!inter) return res;
            var segments = new List<DirectedSegment3>();
            segments.AddRange(f1.Outer);
            foreach (var inner in f1.Inners)
            {
                segments.AddRange(inner);
            }
            segments.AddRange(f2.Outer);
            foreach (var inner in f2.Inners)
            {
                segments.AddRange(inner);
            }
            var intersections = new List<Point3>();
            foreach (var segment in segments)
            {
                Point3 intersection;
                inter = LineIntersection(segment.P1, segment.Direction, p, dir, out intersection);
                if (inter) intersections.Add(intersection);
            }
            if (intersections.Count <= 1) return res;
            intersections = SortInLine3(intersections);
            var prev = intersections[0];
            for (var i = 1; i < intersections.Count; i++)
            {
                var current = intersections[i];
                if (f1.IsPointInSurface(prev + (current - prev)/2, includingBorder) &&
                    f2.IsPointInSurface(prev + (current - prev)/2, includingBorder))
                {
                    res.Add(new DirectedSegment3(prev, current));
                }
                prev = current;
            }
            return res;
        }
    }
}
