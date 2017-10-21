using System;
using System.Diagnostics;
using Geometry.Arithmetic;

namespace Geometry.G2D
{
    public class Vector2
    {
        public readonly double X, Y;
        public static readonly Vector2 X_AXIS = new Vector2(1, 0);
        public static readonly Vector2 Y_AXIS = new Vector2(0, 1);

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.X, -v.Y);
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 operator *(Vector2 v, double f)
        {
            return new Vector2(v.X*f, v.Y*f);
        }

        public static Vector2 operator *(double f, Vector2 v)
        {
            return v*f;
        }

        public static Vector2 operator /(Vector2 v, double scale)
        {
            Debug.Assert(!scale.Equals(0), "Vector divided zero");
#if !NO_EXCEPTION
            if (scale.Equals(0)) throw new DivideZeroException("vector could not divide zero");
#endif
            return new Vector2(v.X/scale, v.Y/scale);
        }

        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            if ((object) v1 == null && (object) v2 == null) return true;
            if ((object) v1 == null) return false;
            if ((object) v2 == null) return false;
            return v1.X.Near(v2.X) && v1.Y.Near(v2.Y);
        }

        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            var v = obj as Vector2;
            if ((object) v == null) return false;
            return X.Near(v.X) && Y.Near(v.Y);
        }

        protected bool Equals(Vector2 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        // Auto generated code.
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode()*397) ^ Y.GetHashCode();
            }
        }

        public static double Dot(Vector2 v1, Vector2 v2)
        {
            return v1.X*v2.X + v1.Y*v2.Y;
        }

        // It has no geometrical mean. Just for convinience.
        public static double Cross(Vector2 v1, Vector2 v2)
        {
            return v1.X*v2.Y - v1.Y*v2.X;
        }

        public double Length => Math.Sqrt(X*X + Y*Y);

        public Vector2 Normalize()
        {
            Debug.Assert(!Length.Equals(0), "Zero vector could not be normalized.");
#if !NO_EXCEPTION
            if (Length.Equals(0)) throw new ArithmeticException("Zero vector could not be normalized.");
#endif
            return this/Length;
        }

        // The forward pi/2 one.
        public Vector2 Orthogonalize()
        {
#if !NO_EXCEPTION
            if (Length.Equals(0)) throw new ArithmeticException("Zero vector has no orthogonal vector.");
#endif
            return new Vector2(-Y/Length, X/Length);
        }

        public Vector2 Rotate(double angle)
        {
            return new Vector2(X*Math.Cos(angle) - Y*Math.Sin(angle),
                X*Math.Sin(angle) + Y*Math.Cos(angle));
        }

        public static Vector2 FromPolarAngle(double polarAngle)
        {
            return new Vector2(Math.Cos(polarAngle), Math.Sin(polarAngle));
        }

        public static double ToPolarAngle(Vector2 v)
        {
            return v.ToPolarAngle();
        }

        public double ToPolarAngle()
        {
            return Math.Atan2(Y, X);
        }

        public override string ToString()
        {
            return $"Vector2<{X}, {Y}>";
        }
    }

    public class Point2
    {
        public readonly double X, Y;

        public static readonly Point2 ZERO_POINT = new Point2(0, 0);

        public Point2(double x, double y)
        {
            X = x;
            Y = y;
        }

        private Point2(Point2 p)
        {
            X = p.X;
            Y = p.Y;
        }

        public static Vector2 operator -(Point2 p1, Point2 p2)
        {
            return new Vector2(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point2 operator +(Point2 p, Vector2 v)
        {
            return new Point2(p.X + v.X, p.Y + v.Y);
        }

        public static Point2 operator -(Point2 p, Vector2 v)
        {
            return new Point2(p.X - v.X, p.Y - v.Y);
        }

        public double DistanceTo(Point2 other)
        {
            return (this - other).Length;
        }

        public double DistanceTo(DirectedSegment2 segment)
        {
            return DistanceToSegment(segment.P1, segment.P2);
        }

        public static double Distance(Point2 p1, Point2 p2)
        {
            return p1.DistanceTo(p2);
        }

        public double DistanceToSegment(Point2 sp1, Point2 sp2)
        {
            if (sp1 == sp2) return DistanceTo(sp1);
            var v1 = sp2 - sp1;
            var v2 = this - sp1;
            var v3 = this - sp2;
            if (Vector2.Dot(v1, v2).DCompareTo(0) < 0) return v2.Length;
            if (Vector2.Dot(v1, v3).DCompareTo(0) > 0) return v3.Length;
            return Math.Abs(Vector2.Cross(v1, v2))/v1.Length;
        }

        public Point2 Move(Vector2 direction, double distance)
        {
            return this + direction*distance;
        }

        public Point2 Move(double polarAngle, double distance)
        {
            return Move(new Vector2(Math.Cos(polarAngle), Math.Sin(polarAngle)), distance);
        }

        public override string ToString()
        {
            return $"Point2({X}, {Y})";
        }
    }
}
