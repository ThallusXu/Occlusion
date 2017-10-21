using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Geometry.Arithmetic;
using Geometry.G3D;

namespace OcclusionApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var q = new Quaternion(new EulerAngle(Constants.PI/6, Constants.PI/12, 0));
            var ps = new List<List<Point3>>
            {
                new List<Point3>
                {
                    new Point3(0, 0, 0), new Point3(0, 0, 100)
                },
                new List<Point3>
                {
                    new Point3(0, 0, 100), new Point3(100, 0, 100)
                },
                new List<Point3>
                {
                    new Point3(100, 0, 100), new Point3(100, 0, 0)
                },
                new List<Point3>
                {
                    new Point3(100, 0, 0), new Point3(0, 0, 0)
                }
            };
            var plane = new FlatPlane(ps);
            var prism = new Prism(plane, new Vector3(0, 0, 0), new Vector3(0, 100, 0));
            prism = prism.Rotate(q);
            //var segs = Occlusion.Occlude(new List<Prism> { prism });

            var ps2 = new List<List<Point3>>
            {
                new List<Point3>
                {
                    new Point3(100.000000, 0, 150.000000),
                    new Point3(96.077045, 0, 149.845867),
                    new Point3(92.178277, 0, 149.384417),
                    new Point3(88.327732, 0, 148.618496),
                    new Point3(84.549150, 0, 147.552826),
                    new Point3(80.865828, 0, 146.193977),
                    new Point3(77.300475, 0, 144.550326),
                    new Point3(73.875072, 0, 142.632008),
                    new Point3(70.610737, 0, 140.450850),
                    new Point3(67.527598, 0, 138.020298),
                    new Point3(64.644661, 0, 135.355339),
                    new Point3(61.979702, 0, 132.472402),
                    new Point3(59.549150, 0, 129.389263),
                    new Point3(57.367992, 0, 126.124928),
                    new Point3(55.449674, 0, 122.699525),
                    new Point3(53.806023, 0, 119.134172),
                    new Point3(52.447174, 0, 115.450850),
                    new Point3(51.381504, 0, 111.672268),
                    new Point3(50.615583, 0, 107.821723),
                    new Point3(50.154133, 0, 103.922955),
                    new Point3(50.000000, 0, 100.000000),
                },
                new List<Point3>
                {
                    new Point3(50, 0, 100), new Point3(100, 0, 100),
                },
                new List<Point3>
                {
                    new Point3(100, 0, 100), new Point3(100, 0, 150)
                }
            };
            var plane2 = new FlatPlane(ps2);
            var prism2 = new Prism(plane2, new Vector3(0, 0, 0), new Vector3(0, 50, 0));
            prism2 = prism2.Rotate(q);
            var segs = Occlusion.Occlude(new List<Prism> { prism , prism2 });

            var image = new Bitmap(600, 600);
            using (var g = Graphics.FromImage(image))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Lavender);
                Color[] colors = 
                {Color.Black, Color.Blue, Color.Aqua, Color.Brown, Color.Chartreuse, Color.Chocolate, Color.DarkGreen,};
                var i = 0;
                foreach (var ds in segs)
                {
                    var pen = new Pen(colors[(i++%colors.Length)], 2);
                    g.DrawLine(pen, new PointF((float)ds.P1.X + 300, -(float)ds.P1.Y + 300), new PointF((float)ds.P2.X + 300, -(float)ds.P2.Y + 300));
                }
                g.Save();
            }
            image.Save("test.bmp");
        }
    }
}
