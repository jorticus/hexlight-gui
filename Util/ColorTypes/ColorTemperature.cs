using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Util.ColorTypes
{
    struct ColorTemperature
    {
        public float k; // Temperature in kelvins

        #region Constants

        public static ColorTemperature Hot { get { return new ColorTemperature(1000); } }
        public static ColorTemperature Warm { get { return new ColorTemperature(2200); } }
        public static ColorTemperature Neutral { get { return new ColorTemperature(4500); } }
        public static ColorTemperature Cool { get { return new ColorTemperature(9000); } }
        public static ColorTemperature Cold { get { return new ColorTemperature(11000); } }

        #endregion

        public ColorTemperature(float temperature)
        {
            this.k = temperature;
        }

        public RGBColor ToRGB()
        {
            float r, g, b;

            // Red
            if (k < 6600)
            {
                r = 255;
            }
            else
            {
                r = k - 6000.0f;
                r = 329.698727446f * (float)Math.Pow(r / 100.0, -0.1332047592);
            }

            // Green
            if (k < 6600)
            {
                g = k;
                g = 99.4708025861f * (float)Math.Log(g / 100.0) - 161.1195681661f;
            }
            else
            {
                g = k - 6000;
                g = 288.1221695283f * (float)Math.Pow(g / 100.0, -0.0755148492);
            }

            // Blue
            if (k > 6600)
            {
                b = 255;
            }
            else
            {
                //if (K < 1900) 
                b = k - 1000;
                b = 138.5177312231f * (float)Math.Log(b / 100.0) - 305.0447927307f;
            }

            r /= 255.0f;
            g /= 255.0f;
            b /= 255.0f;

            if (r > 1.0f) r = 1.0f;
            if (g > 1.0f) g = 1.0f;
            if (b > 1.0f) b = 1.0f;
            if (r < 0.0f) r = 0.0f;
            if (g < 0.0f) g = 0.0f;
            if (b < 0.0f) b = 0.0f;

            return new RGBColor(r, g, b);
        }

        public static implicit operator RGBColor(ColorTemperature ct)
        {
            return ct.ToRGB();
        }
    }
}
