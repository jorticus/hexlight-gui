using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HexLight.Colour;

namespace HexLight.WpfControls.ColourWheels
{
    class HSVWheel : ColourWheel
    {
        private const double whiteFactor = 2.2; // Provide more accuracy around the white-point

        public override RGBColor ColourMapping(double radius, double theta, double value)
        {
            HSVColor hsv = new HSVColor((float)theta, (float)Math.Pow(radius, whiteFactor), (float)value);
            RGBColor rgb = hsv.ToRGB();
            return rgb;
        }

        public override Point InverseColourMapping(RGBColor rgb)
        {
            double theta, rad;
            HSVColor hsv = (HSVColor)rgb;
            theta = hsv.hue;
            rad = Math.Pow(hsv.sat, 1.0 / whiteFactor);            

            return new Point(theta, rad);
        }
    }
}
