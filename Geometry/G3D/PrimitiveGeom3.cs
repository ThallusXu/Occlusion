using System;
using System.Diagnostics;
using Geometry.Arithmetic;
using Geometry.G2D;

namespace Geometry.G3D
{
    public enum EPlane
    {
        Xoy = 1,
        Xoz,
        Yoz
    }
    
    public interface ITransformational <out T>
    {
        T Rotate(Quaternion quaternion);
        T Move(Vector3 v, double distance);
        T Move(Vector3 v);
    }

    public class Point3 : ITransformational<Point3>
    {
        public readonly double X, Y, Z;
        public static readonly Point3 ZERO_POINT = new Point3(0, 0, 0);

        public Point3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Point3 operator +(Point3 p, Vector3 v)
        {
            return new Point3(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        }

        public static Point3 operator -(Point3 p, Vector3 v)
        {
            return new Point3(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        }

        public static Vector3 operator -(Point3 p1, Point3 p2)
        {
            return new Vector3(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static bool operator ==(Point3 p1, Point3 p2)
        {
            if ((object) p1 == null) return (object) p2 == null;
            if ((object) p2 == null) return false;
            return p1.X.Near(p2.X) && p1.Y.Near(p2.Y) && p1.Z.Near(p2.Z);
        }

        public static bool operator !=(Point3 p1, Point3 p2)
        {
            return !(p1 == p2);
        }

        protected bool Equals(Point3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Point3) obj);
        }

        // Auto generated code
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public double DistanceTo(Point3 p)
        {
            return (this - p).Length;
        }

        public Point3 Rotate(Quaternion quaternion)
        {
            return ZERO_POINT + (this - ZERO_POINT)*quaternion;
        }

        public Point3 Move(Vector3 v, double distance)
        {
            if (v.Length.Equals(0)) throw new ArithmeticException("direct vector should not be zero vector");
            return this + v.Normalize()*distance;
        }

        public Point3 Move(Vector3 v)
        {
            return this + v;
        }

        public Point2 ToPoint2(EPlane plane)
        {
            switch (plane)
            {
                case EPlane.Xoy:
                    return new Point2(X, Y);
                case EPlane.Xoz:
                    return new Point2(X, Z);
                case EPlane.Yoz:
                    return new Point2(Y, Z);
                default:
                    throw new GeometryException("plane must be one of xoy, xoz or yoz");
            }
        }

        public override string ToString()
        {
            return $"Point3({X}, {Y}, {Z})";
        }
    }

    public class Vector3
    {
        public readonly double X, Y, Z;
        public static readonly Vector3 X_AXIS = new Vector3(1, 0, 0);
        public static readonly Vector3 Y_AXIS = new Vector3(0, 1, 0);
        public static readonly Vector3 Z_AXIS = new Vector3(0, 0, 1);

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static double Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X*v2.X + v1.Y*v2.Y + v1.Z*v2.Z;
        }

        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.Y*v2.Z - v1.Z*v2.Y, v1.Z*v2.X - v1.X*v2.Z, v1.X*v2.Y - v1.Y*v2.X);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator *(Vector3 v, double f)
        {
            return new Vector3(v.X*f, v.Y*f, v.Z*f);
        }

        public static Vector3 operator *(double f, Vector3 v)
        {
            return v*f;
        }

        public static Vector3 operator *(Vector3 v, Quaternion q)
        {
            var x = v.X;
            var y = v.Y;
            var z = v.Z;

            var qx = q.X;
            var qy = q.Y;
            var qz = q.Z;
            var qw = q.W;

            var ix = qw * x + qy * z - qz * y;
            var iy = qw * y + qz * x - qx * z;
            var iz = qw * z + qx * y - qy * x;
            var iw = -qx * x - qy * y - qz * z;

            x = ix * qw + iw * -qx + iy * -qz - iz * -qy;
            y = iy * qw + iw * -qy + iz * -qx - ix * -qz;
            z = iz * qw + iw * -qz + ix * -qy - iy * -qx;

            return new Vector3(x, y, z);
        }

        public static Vector3 operator /(Vector3 v, double f)
        {
            Debug.Assert(!f.Equals(0), "Vector divided zero");
#if !NO_EXCEPTION
            if (f.Equals(0)) throw new DivideZeroException("vector could not divide zero");
#endif
            return new Vector3(v.X/f, v.Y/f, v.Z/f);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            if ((object) v1 == null) return (object) v2 == null;
            if ((object) v2 == null) return false;
            return v1.X.Near(v2.X) && v1.Y.Near(v2.Y) && v1.Z.Near(v2.Z);
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !(v1 == v2);
        }

        protected bool Equals(Vector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Vector3) obj);
        }

        // Auto generated code
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public double Length => Math.Sqrt(X*X + Y*Y + Z*Z);

        public Vector3 Normalize()
        {
            Debug.Assert(!Length.Equals(0));
#if !NO_EXCEPTION
            if (Length.Equals(0)) throw new ArithmeticException("Zero vector could not be normalized.");
#endif
            return new Vector3(X/Length, Y/Length, Z/Length);
        }

        public override string ToString()
        {
            return $"Vector3<{X}, {Y}, {Z}>";
        }
    }

    public class Quaternion
    {
        public readonly double X, Y, Z, W;

        public Quaternion(EulerAngle ua)
        {
            var c1 = Math.Cos(ua.X/2);
            var c2 = Math.Cos(ua.Y/2);
            var c3 = Math.Cos(ua.Z/2);

            var s1 = Math.Sin(ua.X/2);
            var s2 = Math.Sin(ua.Y/2);
            var s3 = Math.Sin(ua.Z/2);

            switch (ua.Order)
            {
                case EulerAngle.EOrder.Xyz:
                    X = s1*c2*c3 + c1*s2*s3;
                    Y = c1*s2*c3 - s1*c2*s3;
                    Z = c1*c2*s3 + s1*s2*c3;
                    W = c1*c2*c3 - s1*s2*s3;
                    break;
                case EulerAngle.EOrder.Xzy:
                    X = s1*c2*c3 - c1*s2*s3;
                    Y = c1*s2*c3 - s1*c2*s3;
                    Z = c1*c2*s3 + s1*s2*c3;
                    W = c1*c2*c3 + s1*s2*s3;
                    break;
                case EulerAngle.EOrder.Yxz:
                    X = s1*c2*c3 + c1*s2*s3;
                    Y = c1*s2*c3 - s1*c2*s3;
                    Z = c1*c2*s3 - s1*s2*c3;
                    W = c1*c2*c3 + s1*s2*s3;
                    break;
                case EulerAngle.EOrder.Yzx:
                    X = s1*c2*c3 + c1*s2*s3;
                    Y = c1*s2*c3 + s1*c2*s3;
                    Z = c1*c2*s3 - s1*s2*c3;
                    W = c1*c2*c3 - s1*s2*s3;
                    break;
                case EulerAngle.EOrder.Zxy:
                    X = s1*c2*c3 - c1*s2*s3;
                    Y = c1*s2*c3 + s1*c2*s3;
                    Z = c1*c2*s3 + s1*s2*c3;
                    W = c1*c2*c3 - s1*s2*s3;
                    break;
                case EulerAngle.EOrder.Zyx:
                    X = s1*c2*c3 - c1*s2*s3;
                    Y = c1*s2*c3 + s1*c2*s3;
                    Z = c1*c2*s3 - s1*s2*c3;
                    W = c1*c2*c3 + s1*s2*s3;
                    break;
            }
        }
    }

    public class EulerAngle
    {
        public readonly double X, Y, Z;
        public readonly EOrder Order;

        public enum EOrder
        {
            Xyz,
            Xzy,
            Yzx,
            Yxz,
            Zxy,
            Zyx
        }

        public EulerAngle(double x, double y, double z, EOrder order = EOrder.Xyz)
        {
            X = x;
            Y = y;
            Z = z;
            Order = order;
        }
    }

    public class CoordinateSystem3
    {
        public readonly Vector3 AxisX, AxisY, AxisZ;
        public readonly Point3 Origin;

        public CoordinateSystem3(Point3 origin, Vector3 x, Vector3 y, Vector3 z)
        {
            AxisX = x;
            AxisY = y;
            AxisZ = z;
            Origin = origin;
        }

        public CoordinateSystem3(Point3 p1, Point3 p2, Point3 p3)
        {
            Origin = p1;
            AxisX = (p1 - p2).Normalize();
            AxisZ = Vector3.Cross(AxisX, p3 - p1);
            AxisY = Vector3.Cross(AxisZ, AxisY);
        }

        public Point3 ToLocal(Point3 p)
        {
            var v = p - Origin;
            return new Point3(Vector3.Dot(v, AxisX), Vector3.Dot(v, AxisY), Vector3.Dot(v, AxisZ));
        }

        public Point3 FromLocal(Point3 p)
        {
            return Origin + AxisX*p.X + AxisY*p.Y + AxisZ*p.Z;
        }
    }
}
