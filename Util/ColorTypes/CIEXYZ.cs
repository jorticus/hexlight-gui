using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RGB.Util.ColorTypes
{


    public struct CIEXYZColour
    {
        public float X, Y, Z;

        public CIEXYZColour(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        #region Implicit Conversion

        public RGBColor ToRGB()
        {
            // NOTE: Assumes linear RGB, not sRGB.
            float r, g, b;
            r = (3.2406f * X) + (-1.5372f * Y) + (-0.4986f * Z);
            g = (-0.9689f * X) + (1.8758f * Y) + (0.0415f * Z);
            b = (0.0557f * X) + (-0.2040f * Y) + (1.0570f * Z);
            return new RGBColor(r, g, b);
        }

        public static CIEXYZColour FromRGB(RGBColor rgb)
        {
            // NOTE: Assumes linear RGB, not sRGB.
            float x, y, z;
            x = (0.4124f * rgb.r) + (0.3576f * rgb.g) + (0.1805f * rgb.b);
            y = (0.2126f * rgb.r) + (0.7152f * rgb.g) + (0.0722f * rgb.b);
            z = (0.0193f * rgb.r) + (0.1192f * rgb.g) + (0.9502f * rgb.b);
            return new CIEXYZColour(x, y, z);
        }

        public static implicit operator RGBColor(CIEXYZColour xyz)
        {
            return xyz.ToRGB();
        }

        public static implicit operator CIEXYZColour(RGBColor rgb)
        {
            return CIEXYZColour.FromRGB(rgb);
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

    public struct CIExyYColor
    {
        public float x, y, Y;

        public CIExyYColor(float x, float y, float Y)
        {
            this.x = x;
            this.y = y;
            this.Y = Y;
        }

        #region Implicit Conversion

        public static implicit operator CIEXYZColour(CIExyYColor xyY)
        {
            if (xyY.y == 0.0f) {
                return new CIEXYZColour(0, 0, 0);
            }
            else {
                float X, Z;
                X = (xyY.Y / xyY.y) * xyY.x;
                Z = (xyY.Y / xyY.y) * (1 - xyY.x - xyY.y);
                return new CIEXYZColour(X, xyY.Y, Z);
            }
        }

        public static implicit operator CIExyYColor(CIEXYZColour xyz)
        {
            float x, y, s;
            s = (xyz.X + xyz.Y + xyz.Z);
            x = xyz.X / s;
            y = xyz.Y / s;
            return new CIExyYColor(x, y, xyz.Y);
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
