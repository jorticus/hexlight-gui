using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;

namespace HexLight.WpfControls.ColourWheels
{
    class HybridWheel : ColourWheel
    {
        public override RGBColor ColourMapping(double radius, double theta, double value)
        {
            return new RGBColor(1.0f, 0.0f, 0.0f);
        }
    }
}
