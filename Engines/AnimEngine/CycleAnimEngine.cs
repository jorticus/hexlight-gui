using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;

namespace HexLight.Engine.Anim
{
    public partial class AnimEngine
    {
        private double cycleHue;

        private HSVColor CycleHueUpdate(double tickRate)
        {
            cycleHue += Speed * tickRate;
            return new HSVColor((float)cycleHue, 1.0f, 1.0f);
        }
    }
}
