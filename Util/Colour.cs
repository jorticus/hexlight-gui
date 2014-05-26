using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media;

namespace RGB.Util
{
    public interface IColour
    {

    }

    /// <summary>
    /// Custom colour utility class.
    /// Provides various conversion methods and operations.
    /// </summary>
    public class Colour  // Colour is the correct spelling.
    {

        #region Static Members
        public static Colour Interpolate(Colour c1, Colour c2, double value)
        {
            double v1 = 1.0 - value;
            double v2 = value;
            return new Colour(
                c1.R * v1 + c2.R * v2,
                c1.G * v1 + c2.G * v2,
                c1.B * v1 + c2.B * v2,
                c1.A * v1 + c2.A * v2
            );
        }
        #endregion

        #region RGBA Float
        private double r = 0, g = 0, b = 0, a = 1;
        public double R { get { return r; } set { r = value; } }
        public double G { get { return g; } set { g = value; } }
        public double B { get { return b; } set { b = value; } }
        public double A { get { return a; } set { a = value; } }
        public double Brightness { get; set; }
        #endregion

        #region RGBA Byte
        public byte Rb { get { return FloatToByte(r); } set { r = ByteToFloat(value); } }
        public byte Gb { get { return FloatToByte(g); } set { g = ByteToFloat(value); } }
        public byte Bb { get { return FloatToByte(b); } set { b = ByteToFloat(value); } }
        public byte Ab { get { return FloatToByte(a); } set { a = ByteToFloat(value); } }
        #endregion

        #region HSV
        private double h = 0, s = 1, v = 1;
        public double H { get { return h; } set { h = value; UpdateHSV(h, s, v); } }
        public double S { get { return s; } set { s = value; UpdateHSV(h, s, v); } }
        public double V { get { return v; } set { v = value; UpdateHSV(h, s, v); } }

        // Hue: 0 to 360
        // Saturation: 0 to 1
        // Value: 0 to 1
        public Colour UpdateHSV(double hue, double sat, double value)
        {
            this.h = hue;
            this.s = sat;
            this.v = value;

            int hi = (int)Math.Floor(hue / 60) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = (int)value;
            int p = (int)(value * (1.0 - sat));
            int q = (int)(value * (1.0 - f * sat));
            int t = (int)(value * (1.0 - (1.0 - f) * sat));

            switch (hi)
            {
                case 0: Rb = (byte)v; Gb = (byte)t; Bb = (byte)p; break;
                case 1: Rb = (byte)q; Gb = (byte)v; Bb = (byte)p; break;
                case 2: Rb = (byte)p; Gb = (byte)v; Bb = (byte)t; break;
                case 3: Rb = (byte)p; Gb = (byte)q; Bb = (byte)v; break;
                case 4: Rb = (byte)t; Gb = (byte)p; Bb = (byte)v; break;
                case 5: Rb = (byte)v; Gb = (byte)p; Bb = (byte)q; break;
            }
            
            return this;
        }

        public float GetHue()
        {
            throw new NotImplementedException();
            //return Color.FromArgb(Ab, Rb, Gb, Bb).GetHue();
        }
        public double GetSaturation()
        {
            double max = Math.Max(R, Math.Max(G, B));
            double min = Math.Min(R, Math.Min(G, B));
            return (max == 0.0) ? 0.0 : 1.0 - (1.0 * min / max);
        }
        public double GetValue()
        {
            return Math.Max(R, Math.Max(G, B));
        }

        #endregion

        #region Colour Temperature
        private double k = 0;
        public double K { get { return k; } set { k = value; SetTemperature(value); } }

        public void SetTemperature(double K)
        {
            double r, g, b;

            // Red
            if (K < 6600)
            {
                r = 255;
            }
            else
            {
                r = K - 6000.0;
                r = 329.698727446 * Math.Pow(r / 100.0, -0.1332047592);
            }

            // Green
            if (K < 6600)
            {
                g = K;
                g = 99.4708025861 * Math.Log(g / 100.0) - 161.1195681661;
            }
            else
            {
                g = K - 6000;
                g = 288.1221695283 * Math.Pow(g / 100.0, -0.0755148492);
            }

            // Blue
            if (K > 6600)
            {
                b = 255;
            }
            else
            {
                //if (K < 1900) 
                b = K - 1000;
                b = 138.5177312231 * Math.Log(b / 100.0) - 305.0447927307;
            }



            this.R = Limit(r / 255.0, 0, 1);
            this.G = Limit(g / 255.0, 0, 1);
            this.B = Limit(b / 255.0, 0, 1);
        }
        #endregion

        #region CIE Correction
        // Return CIE1931-Corrected luminance value,
        // eg. for applying an RGB value to an LED.
        private double cie1931(double L)
        {
            L = L * 100.0;
            return (L <= 8.0) ? (L/902.3) : Math.Pow((L + 16.0) / 116.0, 3);
        }

        // Return CIE-corrected values, suitable for driving LEDs
        public Colour ToCIE()
        {
            return new Colour(
                cie1931(this.R),
                cie1931(this.G),
                cie1931(this.B),
                cie1931(this.A)   // Alpha could be used as brightness if you want
            );
        }
        #endregion

        #region Constructors
        public Colour() { }
        public Colour(double R, double G, double B, double A=1.0) { this.R = R; this.G = G; this.B = B; this.A = A; }
        //public Colour(byte R, byte G, byte B) { this.Rb = R; this.Gb = G; this.Bb = B; this.Ab = 255; }
        //public Colour(byte R, byte G, byte B, byte A) { this.Rb = R; this.Gb = G; this.Bb = B; this.Ab = A; }
        /*public Colour(string name)
        {
            //TODO: Convert string name or hexadecimal to colour representation
            Color c = Color.FromName(name);
            this.Rb = c.R;
            this.Gb = c.G;
            this.Bb = c.B;
            this.Ab = c.A;
        }*/
        public Colour(Color c)
        {
            // Backwards compatability
            this.Rb = c.R;
            this.Gb = c.G;
            this.Bb = c.B;
            this.Ab = c.A;
        }
        #endregion

        #region Secondary Constructors

        public static Colour FromRgba(int R, int G, int B, int A = 255)
        {
            Colour c = new Colour();
            c.Rb = (byte)R;
            c.Gb = (byte)G;
            c.Bb = (byte)B;
            c.Ab = (byte)A;
            return c;
        }

        static public Colour FromHex(string hex)
        {
            Colour c = new Colour();
            c.Rb = 0;
            c.Gb = 0;
            c.Bb = 0;
            c.Ab = 255;
            return c; //TODO: complete
        }

        public static Colour FromHsv(double hue, double sat, double value)
        {
            Colour c = new Colour();
            c.UpdateHSV(hue, sat, value);
            return c;
        }

        public static Colour FromTemperature(double K)
        {
            Colour c = new Colour();
            c.SetTemperature(K);
            return c;
        }
        #endregion

        #region Utilities

        public void Update(double R, double G, double B, double A = 1.0f)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }

        // Convert an ARGB to RGB, where A represents the brightness
        public void MultiplyAlpha()
        {
            if (a != 1.0)
            {
                r = r * a;
                g = g * a;
                b = b * a;
                a = 1.0;
            }
        }

        public Color ToColor() { return Color.FromArgb(Rb, Gb, Bb, Ab); }
        //public byte[] ToBytes() { return new byte[]{R,G,B}; }

        #endregion

        #region Private Utilities
        private byte FloatToByte(double value)
        {
            if (value < 0.0) return 0;
            if (value > 1.0) return 255;
            return (byte)(value * 255.0);
        }

        private double ByteToFloat(byte value)
        {
            return (double)value / 255.0;
        }

        private double Limit(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        #endregion

        #region Operators

        // Colour1 + Colour2
        public static Colour operator +(Colour c1, Colour c2)
        {
            return new Colour(
                c1.R + c2.R,
                c1.G + c2.G,
                c1.B + c2.B,
                c1.A + c2.A //TODO: does this make sense?
            );
        }

        // Colour1 * Colour 2
        public static Colour operator *(Colour c1, Colour c2)
        {
            /*return new Colour(
                c1.R * c2.R,
                c1.G * c2.G,
                c1.B * c2.B,
                c1.A * c2.A
            );*/
            return new Colour(
                (c1.R * c1.A) + (c2.R * c2.A),
                (c1.G * c1.A) + (c2.G * c2.A),
                (c1.B * c1.A) + (c2.B * c2.A),
                c1.A + c2.A
            );
        }

        // Colour1 + float
        public static Colour operator +(Colour c, float f)
        {
            return new Colour(
                c.R + f,
                c.G + f,
                c.B + f,
                c.A// + f
            );
        }

        // Colour1 * float
        public static Colour operator *(Colour c, float f)
        {
            return new Colour(
                c.R * f,
                c.G * f,
                c.B * f,
                c.A * f
            );
        }

        // Compatability between System.Drawing.Color
        // eg: Color c = new Colour(1,0,0);
        public static implicit operator Color(Colour c)
        {
            return Color.FromArgb(c.Ab, c.Rb, c.Gb, c.Bb); //Note: ARGB, not RGBA
        }
        // Colour c = Color.Red;
        // Colour c = Color.FromName("Firebrick");
        public static implicit operator Colour(Color c)
        {
            return new Colour(c);
        }

        //public static Colour operator 
        #endregion

        #region ToString Methods

        public string ToHexRGBA()
        {
            return String.Format("#{0:x2}{1:x2}{2:x2}{3:x2}", Rb, Gb, Bb, Ab);
        }

        public string ToHexRGB()
        {
            return String.Format("#{0:x2}{1:x2}{2:x2}", Rb, Gb, Bb);
        }

        public override string ToString()
        {
            return String.Format("rgba({0:0.00},{1:0.00},{2:0.00},{3:0.00})", R, G, B, A);
        }

        #endregion

        #region Blending

        // Overlays this colour including alpha onto another solid colour.
        public Colour Overlay(Colour other)
        {
            double A1 = this.A;
            double A2 = 1.0 - this.A;
            this.A = 1.0f;

            this.R = (R * A1) + (other.R * A2);
            this.G = (G * A1) + (other.G * A2);
            this.B = (B * A1) + (other.B * A2);
            this.A = 1.0;

            return this;
        }

        #endregion
    }
}
