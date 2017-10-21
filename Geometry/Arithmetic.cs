using System;

namespace Geometry
{
    namespace Arithmetic
    {
        public static class Constants
        {
            public const double PI = Math.PI;
            public const double PI_2 = PI/2;
            public const double TWO_PI = PI*2;

            public const double DEFAULT_EPS = 1e-3;
        }

        public static class DoubleExtension
        {
            public static int DCompareTo(this double x, double y, double eps = Constants.DEFAULT_EPS)
            {
                var diff = x - y;
                if (diff > eps) return 1;
                if (diff < -eps) return -1;
                return 0;
            }

            public static bool Near(this double x, double y, double eps = Constants.DEFAULT_EPS)
            {
                return x.DCompareTo(y, eps) == 0;
            }
        }

        public static class Utils
        {
            public static double NormalizeAngle(double rad)
            {
                return rad - Constants.TWO_PI*Math.Floor((rad + Constants.PI)/Constants.TWO_PI);
            }

            public static bool AngleEqual(double r1, double r2, double threshold = Constants.DEFAULT_EPS)
            {
                var diff = NormalizeAngle(r1 - r2);
                return diff.Near(0, threshold);
            }
        }
    }
}