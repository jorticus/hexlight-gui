﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGB.Util.ColorTypes;

namespace RGB.Util
{
    class CIE1931
    {
        /// <summary>
        /// Apply CIE intensity perception to the given lumininace value
        /// </summary>
        /// <param name="L">The luminance, between 0.0 and 1.0</param>
        /// <returns>Percieved intensity, between 0.0 and 1.0</returns>
        public static float LumToBrightness(float L)
        {
            // See: http://en.wikipedia.org/wiki/CIELUV#The_reverse_transformation
            // (Yn is assumed to be 1.0)
            return (L <= 0.08f) ? (L / 9.033f) : (float)Math.Pow((L + 0.16f) / 1.16f, 3);
        }

        /// <summary>
        /// Apply CIE correction to an RGB colour.
        /// This is useful for driving LEDs, as their output is linear, but our perception to light is not.
        /// </summary>
        /// <param name="input">The colour to correct</param>
        /// <returns>CIE corrected colour</returns>
        public static RGBColor CorrectRGB(RGBColor input)
        {
            return new RGBColor(
                LumToBrightness(input.r),
                LumToBrightness(input.g),
                LumToBrightness(input.b)
            );
        }

    }
}
