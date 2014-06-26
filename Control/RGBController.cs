using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HexLight.Util.ColorTypes;

namespace HexLight.Control
{
    /// <summary>
    /// This defines the minimum required implementation for any RGB controller interface
    /// </summary>
    public abstract class RGBController
    {
        public RGBColor Whitebalance { get; set; }

        #region Abstract Definitions

        /// <summary>
        /// Updates the colour of the LED lights, and returns the current value of the lights.
        /// </summary>
        public abstract RGBColor Color { get; set; }

        public abstract float Brightness { get; set; }

        /// <summary>
        /// If true, we're allowed to update the LEDs
        /// </summary>
        public bool Connected { get; protected set; }

        #endregion

        #region Utility Methods
        //NOTE: It may be better to move these to a separate AnimationController class in the future.
        // That way, the class can deal with animation asynchronously if required, or 
        // possibly use device-specific functions for animating on the hardware itself.

        public virtual void FadeOut(double time)
        {
            FadeTo(Colors.Black, time);
        }
        public virtual void FadeTo(RGBColor dest, double time, bool blocking = true)
        {
            if (blocking)
            {
                RGBColor start = this.Color;

                int tstart = System.Environment.TickCount;
                int tend = tstart + (int)Math.Round(time*1000.0);
                int t = tstart;

                while (t < tend)
                {
                    float x = ((float)(t - tstart) / (float)(tend - tstart));
                    this.Color = RGBColor.Interpolate(start, dest, x);

                    System.Threading.Thread.Sleep(1000 / 60);
                    t = System.Environment.TickCount;
                }
            }

            
            Color = dest;
        }

        #endregion
    }
}
