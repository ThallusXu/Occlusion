using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry.Arithmetic;
using Geometry.G2D;

namespace Geometry.G3D
{
    public static class Occlusion
    {
        private static List<DirectedSegment3> BreakSegmentsBySegments(List<DirectedSegment3> segments)
        {
            var res = new List<DirectedSegment3>();
            for (var i = 0; i < segments.Count; i++)
            {
                var target = segments[i];
                var target2D = target.ToSegment2(EPlane.Xoy);
                if (target2D.Length.Near(0)) continue;
                var intersections = new List<double>();
                for (var j = 1; j < segments.Count; j++)
                {
                    var breaker = segments[(i + j)%segments.Count];
                    var breaker2D = breaker.ToSegment2(EPlane.Xoy);
                    if (breaker2D.Length.Near(0)) continue;
                    if (!Vector2.Cross(target2D.Direction, breaker2D.Direction).Near(0))
                    {
                        var inter = Utils2.GetLineIntersection(target2D.P1, target2D.Direction, breaker2D.P1,
                                                               breaker2D.Direction);
                        if (target2D.Contains(inter)) intersections.Add((inter - target2D.P1).Length / target2D.Length);
                    }
                    //if (DirectedSegment2.IsSegmentsProperIntersection(target2D, breaker2D))
                    //{
                    //    var inter = Utils2.GetLineIntersection(target2D.P1, target2D.Direction, breaker2D.P1,
                    //                                           breaker2D.Direction);
                    //    intersections.Add((inter - target2D.P1).Length/target2D.Length);
                    //}
                }
                intersections.Add(1);
                intersections.Sort();
                var prev = target.P1;
                foreach (var current in intersections.Select(f => target.P1 + target.Direction*f*target.Length)) {
                    res.Add(new DirectedSegment3(prev, current));
                    prev = current;
                }
            }
            return res;
        }

        public static List<DirectedSegment3> Occlude(List<Prism> prisms)
        {
            var segments = prisms.SelectMany(prism => prism.DecomposeToSegments());
            var broken = BreakSegmentsBySegments(segments.ToList());
            var surfaces = prisms.SelectMany(surface => surface.DecomposeToSimpleSurfaces()).ToList();
            var list = new List<DirectedSegment3>();
            foreach (var segment in broken)
            {
                var flag = true;
                foreach (var simpleSurface in surfaces)
                {
                    if (Utils.IsPointOccluded(segment.Center, simpleSurface, Vector3.Z_AXIS))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) list.Add(segment);
            }
            return list;
        }

        public static List<DirectedSegment3> Occlude(SimpleSurface surface, DirectedSegment3 segment)
        {
            var segments = surface.Outer.ToList();
            segments.Add(segment);
            var broken = BreakSegmentsBySegments(segments);
            var res = new List<DirectedSegment3>();
            foreach (var ds in broken)
            {
                if (!Utils.IsPointOccluded(ds.Center, surface, Vector3.Z_AXIS))
                {
                    res.Add(ds);
                }
            }
            return res;
        }
    }
}
