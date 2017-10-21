using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry.G3D
{
    public class Prism : ITransformational<Prism>
    {
        private List<ISurface> _surfaces;

        public Prism(FlatPlane plane, Vector3 backward, Vector3 forward)
        {
            _surfaces = new List<ISurface>();
            if (plane.Normal != (forward - backward).Normalize()) plane.Reverse();
            _surfaces.AddRange(plane.Outer.Select(edge => new RuledSurface(edge, backward, forward)));
            _surfaces.AddRange(plane.Inners.SelectMany(inner => inner.Select(edge => new RuledSurface(edge, backward, forward))));
            _surfaces.Add(plane.Move(backward));
            var top = plane.Move(forward) as FlatPlane;
            top.Reverse();
            _surfaces.Add(top);
        }

        private Prism() {}
        
        public Prism Rotate(Quaternion quaternion)
        {
            return new Prism
            {
                _surfaces = _surfaces.Select(surface => surface.Rotate(quaternion)).ToList()
            };
        }

        public Prism Move(Vector3 v, double distance)
        {
            return new Prism
            {
                _surfaces = _surfaces.Select(surface => surface.Move(v, distance)).ToList()
            };
        }

        public Prism Move(Vector3 v)
        {
            return new Prism
            {
                _surfaces = _surfaces.Select(surface => surface.Move(v)).ToList()
            };
        }

        public List<SimpleSurface> DecomposeToSimpleSurfaces()
        {
            return _surfaces.SelectMany(surface => surface.DecomposeToSimpleSurfaces()).ToList();
        }

        public List<DirectedSegment3> DecomposeToSegments()
        {
            return _surfaces.SelectMany(surface => surface.VisibleSegments()).ToList();
        }
    }
}
