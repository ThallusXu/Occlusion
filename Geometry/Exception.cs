using System;

namespace Geometry
{
    public class DivideZeroException : ArithmeticException
    {
        public DivideZeroException(string message) : base(message)
        {
            
        }
    }

    public class GeometryException : Exception
    {
        public GeometryException(string message) : base(message)
        {

        }
    }
}
