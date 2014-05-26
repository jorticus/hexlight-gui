using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RGB.Util.ColorTypes
{
    public struct RGBColor
    {
        public float r, g, b;

        public byte Rb { get { return FloatToByte(r); } set { r = ByteToFloat(value); } }
        public byte Gb { get { return FloatToByte(g); } set { g = ByteToFloat(value); } }
        public byte Bb { get { return FloatToByte(b); } set { b = ByteToFloat(value); } }
        //public float R { get { return r; } set { r = value; } }

        public RGBColor(float red, float green, float blue)
        {
            r = red;
            g = green;
            b = blue;
        }

        #region Private Utilities
        private byte FloatToByte(double value)
        {
            if (value < 0.0) return 0;
            if (value > 1.0) return 255;
            return (byte)(value * 255.0);
        }

        private float ByteToFloat(byte value)
        {
            return (float)value / 255.0f;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Clamps the RGB values between 0.0 and 1.0
        /// </summary>
        public void Clamp()
        {
            if (r > 1.0f) r = 1.0f;
            if (g > 1.0f) g = 1.0f;
            if (b > 1.0f) b = 1.0f;
            if (r < 0.0f) r = 0.0f;
            if (g < 0.0f) g = 0.0f;
            if (b < 0.0f) b = 0.0f;
        }

        #endregion

        #region Implicit Conversion

        public HSVColor ToHSV()
        {
            float max = (float)Math.Max(r, Math.Max(g, b));
            float min = (float)Math.Min(r, Math.Min(g, b));

            HSVColor hsv = new HSVColor();
            hsv.value = max;

            float delta = max - min;

            if (max != 0)
            {
                hsv.sat = delta / max;
            }
            else
            {
                // r = g = b = 0
                hsv.sat = 0;
                hsv.hue = float.NaN; // Undefined
                return hsv;
            }

            if (this.r >= 1.0f)
                hsv.hue = (this.g - this.b) / delta;    // Between yellow and magenta
            else if (this.g >= 1.0f)
                hsv.hue = 2 + (this.b - this.r) / delta; // Between cyan and yellow
            else
                hsv.hue = 4 + (this.r - this.g) / delta; // Between magenta and cyan

            hsv.hue *= 60.0f; // degrees
            if (hsv.hue < 0)
                hsv.hue += 360;

            return hsv;
        }

        public float GetSaturation()
        {
            float max = (float)Math.Max(r, Math.Max(g, b));
            float min = (float)Math.Min(r, Math.Min(g, b));
            return (max == 0.0f) ? 0.0f : 1.0f - (1.0f * min / max);
        }

        public float GetValue()
        {
            return Math.Max(r, Math.Max(g, b));
        }

        public static implicit operator HSVColor(RGBColor rgb)
        {
            return rgb.ToHSV();
        }

        public static implicit operator RGBColor(Color c)
        {
            return new RGBColor(c.ScR, c.ScG, c.ScB);
        }

        public static implicit operator Color(RGBColor c)
        {
            return new Color() { ScR = c.r, ScG = c.g, ScB = c.b, ScA = 1.0f };
        }

        #endregion

        #region operators

        // Colour1 + Colour2
        public static RGBColor operator +(RGBColor c1, RGBColor c2)
        {
            return new RGBColor(
                c1.r + c2.r,
                c1.g + c2.g,
                c1.b + c2.b
            );
        }

        // Colour1 * Color 2
        public static RGBColor operator *(RGBColor c1, RGBColor c2)
        {
            return new RGBColor(
                c1.r * c2.r,
                c1.g * c2.g,
                c1.b * c2.b
            );
        }

        // Colour1 + float
        public static RGBColor operator +(RGBColor c, float f)
        {
            return new RGBColor(
                c.r + f,
                c.g + f,
                c.b + f
            );
        }

        // Colour1 * float
        public static RGBColor operator *(RGBColor c, float f)
        {
            return new RGBColor(
                c.r * f,
                c.g * f,
                c.b * f
            );
        }

        public static RGBColor Interpolate(RGBColor c1, RGBColor c2, float value)
        {
            return (c1 * (1.0f - value)) + (c2 * value);
        }

        #endregion

        #region ToString Methods

        public string ToHexRGB()
        {
            return String.Format("#{0:x2}{1:x2}{2:x2}", Rb, Gb, Bb);
        }

        public override string ToString()
        {
            return String.Format("rgb({0:0.00},{1:0.00},{2:0.00})", r, g, b);
        }

        #endregion
    }
}
