/*
 * CIE XYZ colour space implementation
 * Author: Jared Sanson
 * 
 * Provides a way to specify device-independant colours, in a linear colourspace.
 * 
 * The default conversion from XYZ to RGB uses the primary colours as specified in the BT.709/sRGB standard:
 * http://www.itu.int/dms_pubrec/itu-r/rec/bt/R-REC-BT.709-5-200204-I!!PDF-E.pdf
 * Note that the RGB returned is NOT sRGB. It is linear!
 * 
 * Useful calculator here:
 * http://www.brucelindbloom.com/index.html?ColorCalcHelp.html
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace RGB.Util.ColorTypes
{
    /// <summary>
    /// Defines the RGB primaries to use when converting XYZ to RGB colourspace.
    /// This is because XYZ is device-independant.
    /// </summary>
    public class CIERGBDefinition
    {
        // Primaries as defined in BT709 standard:
        public static readonly CIERGBDefinition sRGB = new CIERGBDefinition(
            new CIEXYYColor(0.64, 0.33/*, 0.212673*/),  // red
            new CIEXYYColor(0.30, 0.60/*, 0.715152*/),  // green
            new CIEXYYColor(0.15, 0.06/*, 0.072175*/),  // blue
            new CIEXYYColor(0.3127, 0.3290) // reference white (D65)
        );

        // Primaries as defined by the CIE1931 standard:
        public static readonly CIERGBDefinition CIERGB = new CIERGBDefinition(
            new CIEXYYColor(0.735, 0.265, 0.176204),
            new CIEXYYColor(0.274, 0.717, 0.812985),
            new CIEXYYColor(0.167, 0.009, 0.010811),
            new CIEXYYColor(1/3.0, 1/3.0)
        );

        public CIEXYZColour Red;
        public CIEXYZColour Green;
        public CIEXYZColour Blue;
        public CIEXYZColour White;

        public CIERGBDefinition(CIEXYZColour red, CIEXYZColour green, CIEXYZColour blue, CIEXYZColour white)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.White = white;
        }

        public Matrix<double> GetRGBModel()
        {
            var m = DenseMatrix.OfArray(new double[,] {
                {Red.X, Green.X, Blue.X},
                {Red.Y, Green.Y, Blue.Y}, //NB: Y should be 1.0
                {Red.Z, Green.Z, Blue.Z}
            });
            var mi = m.Inverse();
            
            var refwhite = (Vector<double>)White;
            var srgb = mi * refwhite;

            var rgb2xyz = DenseMatrix.OfArray(new double[,] {
                {srgb[0]*m[0,0], srgb[1]*m[0,1], srgb[2]*m[0,2]},
                {srgb[0]*m[1,0], srgb[1]*m[1,1], srgb[2]*m[1,2]},
                {srgb[0]*m[2,0], srgb[1]*m[2,1], srgb[2]*m[2,2]},
            }).Transpose();

            var xyz2rgb = rgb2xyz.Inverse();
            return xyz2rgb;
        }
    }

    /// <summary>
    /// Defines a colour using XYZ tristimulus colourspace. Y is equivalent to luminance, all values must be positive.
    /// All values are linear, but do not represent perceptual linearity.
    /// XYZ and xyY can be implicitly converted between each other.
    /// </summary>
    public struct CIEXYZColour
    {
        public double X, Y, Z;

        public CIEXYZColour(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        #region Implicit Conversion

        public RGBColor ToRGB(CIERGBDefinition primaries)
        {
            // NOTE: Assumes linear RGB, not sRGB.
            var mat = primaries.GetRGBModel();
            var rgb = mat * this;
            return new RGBColor((float)rgb[0], (float)rgb[1], (float)rgb[2]);
        }

        public static CIEXYZColour FromRGB(RGBColor rgb, CIERGBDefinition primaries)
        {
            var mat = primaries.GetRGBModel().Inverse();
            var rgbvec = DenseVector.OfArray(new double[] { rgb.r, rgb.g, rgb.b });
            var xyz = mat * rgbvec;
            return new CIEXYZColour(xyz[0], xyz[1], xyz[2]);
        }

        public static implicit operator RGBColor(CIEXYZColour xyz)
        {
            return xyz.ToRGB(CIERGBDefinition.CIERGB);
        }

        public static implicit operator CIEXYZColour(RGBColor rgb)
        {
            return CIEXYZColour.FromRGB(rgb, CIERGBDefinition.CIERGB);
        }

        public static implicit operator Vector<double>(CIEXYZColour xyz)
        {
            return DenseVector.OfArray(new double[] { xyz.X, xyz.Y, xyz.Z });
        }

        #endregion

        #region operators

        #endregion

        #region ToString Methods

        public override string ToString()
        {
            return String.Format("xyz({0:0.00},{1:0.00},{2:0.00})", X, Y, Z);
        }

        #endregion
    }

    /// <summary>
    /// Defines a colour using xyY colourspace. Y is luminance, xy is chrominance.
    /// All values are linear, but do not represent perceptual linearity.
    /// XYZ and xyY can be implicitly converted between each other.
    /// </summary>
    public struct CIEXYYColor
    {
        public double x, y, Y;

        public CIEXYYColor(double x, double y, double Y=1.0)
        {
            this.x = x;
            this.y = y;
            this.Y = Y;
        }

        #region Implicit Conversion

        public static implicit operator CIEXYZColour(CIEXYYColor xyy)
        {
            if (xyy.y == 0.0f) {
                return new CIEXYZColour(0, 0, 0);
            }
            else {
                double X, Z;
                X = (xyy.Y / xyy.y) * xyy.x;
                Z = (xyy.Y / xyy.y) * (1 - xyy.x - xyy.y);
                return new CIEXYZColour(X, xyy.Y, Z);
            }
        }

        public static implicit operator CIEXYYColor(CIEXYZColour xyz)
        {
            double x, y, s;
            s = (xyz.X + xyz.Y + xyz.Z);
            x = xyz.X / s;
            y = xyz.Y / s;
            return new CIEXYYColor(x, y, xyz.Y);
        }

        public static implicit operator RGBColor(CIEXYYColor xyy)
        {
            return (RGBColor)(CIEXYZColour)xyy;
        }

        #endregion

        #region operators


        #endregion

        #region ToString Methods

        public override string ToString()
        {
            return String.Format("xyz({0:0.00},{1:0.00},{2:0.00})", x, y, Y);
        }

        #endregion
    }
}
