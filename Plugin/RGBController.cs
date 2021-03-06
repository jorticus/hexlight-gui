﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Reflection;
using HexLight.Colour;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Configuration;

namespace HexLight.Control
{
    public class ControllerConnectionException : Exception
    {
        public ControllerConnectionException() { }
        public ControllerConnectionException(string message) : base(message) { }
        public ControllerConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }
    

    /// <summary>
    /// This defines the minimum required implementation for any RGB controller interface
    /// </summary>
    public abstract class RGBController : IDisposable
    {
        public RGBColor Whitebalance { get; set; }
        public ControllerSettings Settings { get; set; }

        #region Abstract Definitions

        public abstract void Connect();

        public abstract void Disconnect();

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

        #region Notifications

        public event EventHandler DisconnectNotification;
        public event EventHandler ConnectNotification;

        #endregion


        #region Private Utilities

        protected void NotifyDisconnect()
        {
            this.Connected = false;
            if (DisconnectNotification != null)
                DisconnectNotification(this, new EventArgs());
        }

        protected void NotifyConnect()
        {
            this.Connected = true;
            if (ConnectNotification != null)
                ConnectNotification(this, new EventArgs());
        }

        #endregion

        public virtual void Dispose()
        {
            
        }
    }
}
