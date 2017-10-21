using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Geometry.Arithmetic;

namespace GeometryTest
{
    [TestClass]
    public class ArithmeticTest
    {
        [TestMethod]
        public void TestNear()
        {
            Assert.IsTrue(.0001.Near(0));
            Assert.IsTrue(.001.Near(0));
            Assert.IsFalse(.01.Near(0));
            Assert.IsTrue(.2.Near(0, .2));

            Assert.IsTrue((-0.0001).Near(0));
            Assert.IsTrue((-0.001).Near(0));
            Assert.IsFalse((-0.01).Near(0));
            Assert.IsTrue((-0.2).Near(0, .2));
        }

        [TestMethod]
        public void TestNormalizeAngle()
        {
            var r = new Random(DateTime.Now.Millisecond);
            var d = r.NextDouble()*Constants.PI;
            Assert.AreEqual(d, Utils.NormalizeAngle(Constants.PI*2 + d), Constants.DEFAULT_EPS);
            Assert.AreEqual(d, Utils.NormalizeAngle(-Constants.PI*2 + d), Constants.DEFAULT_EPS);

            d = -r.NextDouble()*Constants.PI;
            Assert.AreEqual(d, Utils.NormalizeAngle(Constants.PI*2 + d), Constants.DEFAULT_EPS);
            Assert.AreEqual(d, Utils.NormalizeAngle(-Constants.PI*2 + d), Constants.DEFAULT_EPS);
        }

        [TestMethod]
        public void TestAngleEqual()
        {
            var r = new Random(DateTime.Now.Millisecond);
            var d = r.NextDouble()*Constants.PI;
            Assert.IsTrue(Utils.AngleEqual(d, Utils.NormalizeAngle(Constants.PI*2 + d)));
            Assert.IsTrue(Utils.AngleEqual(d, Utils.NormalizeAngle(-Constants.PI*2 + d)));

            d = -r.NextDouble()*Constants.PI;
            Assert.IsTrue(Utils.AngleEqual(d, Utils.NormalizeAngle(Constants.PI*2 + d)));
            Assert.IsTrue(Utils.AngleEqual(d, Utils.NormalizeAngle(-Constants.PI*2 + d)));
        }
    }
}
