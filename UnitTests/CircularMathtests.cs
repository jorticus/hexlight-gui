using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexLight.Util;

namespace UnitTests
{
    [TestClass]
    public class CircularMathTests
    {
        [TestMethod]
        public void TestMod()
        {
            Assert.AreEqual(90, CircularMath.Mod(90));
            Assert.AreEqual(90, CircularMath.Mod(450));
            Assert.AreEqual(90, CircularMath.Mod(-270));
        }

        [TestMethod]
        public void TestBetween1()
        {
            Assert.IsFalse(CircularMath.Between(10, 30, 0));
            Assert.IsTrue(CircularMath.Between(10, 30, 20));
            Assert.IsTrue(CircularMath.Between(10, 30, -340));
            Assert.IsFalse(CircularMath.Between(10, 30, 40));
        }

        [TestMethod]
        public void TestBetween2()
        {
            Assert.IsTrue(CircularMath.Between(-45, 225, 0));
            Assert.IsFalse(CircularMath.Between(-45, 225, 230));
            Assert.IsTrue(CircularMath.Between(-45, 225, -5));
            Assert.IsFalse(CircularMath.Between(-45, 225, 270));
            Assert.IsFalse(CircularMath.Between(-45, 225, -90));
        }

        [TestMethod]
        public void TestNormMap1()
        {
            double start = 0.0;
            double stop = 180.0;
            Assert.AreEqual(0.0, CircularMath.NormMap(start, stop, 0.0));
            Assert.AreEqual(0.5, CircularMath.NormMap(start, stop, 90.0));
            Assert.AreEqual(1.0, CircularMath.NormMap(start, stop, 180.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, 190.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, -10.0));
        }

        [TestMethod]
        public void TestNormMap2()
        {
            double start = 90.0;
            double stop = 270.0;
            Assert.AreEqual(0.0, CircularMath.NormMap(start, stop, 90.0));
            Assert.AreEqual(0.5, CircularMath.NormMap(start, stop, 180.0));
            Assert.AreEqual(1.0, CircularMath.NormMap(start, stop, 270.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, 280.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, 80.0));
        }

        [TestMethod]
        public void TestNormMap3()
        {
            double start = -45.0;
            double stop = 225.0;
            Assert.AreEqual(0.0, CircularMath.NormMap(start, stop, -45.0));
            Assert.AreEqual(1.0/6.0, CircularMath.NormMap(start, stop, 0.0));
            Assert.AreEqual(0.5, CircularMath.NormMap(start, stop, 90.0));
            Assert.AreEqual(1.0, CircularMath.NormMap(start, stop, 225.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, 230.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, -55.0));
        }

        [TestMethod]
        public void TestNormMap4()
        {
            double start = 315.0;
            double stop = 225.0;
            Assert.AreEqual(0.0, CircularMath.NormMap(start, stop, -45.0));
            Assert.AreEqual(0.5, CircularMath.NormMap(start, stop, 90.0));
            Assert.AreEqual(1.0, CircularMath.NormMap(start, stop, 225.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, 230.0));
            Assert.AreEqual(double.NaN, CircularMath.NormMap(start, stop, -55.0));
        }
    }
}
