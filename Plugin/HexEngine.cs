using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using HexLight.Colour;

namespace HexLight.Engine
{
    public class EngineException : Exception
    {
        public EngineException() { }
        public EngineException(string message) : base(message) { }
        public EngineException(string message, Exception innerException) : base(message, innerException) { }
    }

    public abstract class HexEngine
    {
        /// <summary>
        /// Whether the current engine is enabled
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Get a reference to a XAML control to put in the control panel
        /// </summary>
        public abstract UserControl GetControlPage();

        /// <summary>
        /// Enable the engine
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// Disable the engine
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Update the engine colour by 1 tick
        /// </summary>
        /// <param name="tick_time">The time since the last tick, in milliseconds</param>
        /// <returns>The updated colour to send to the controller</returns>
        public abstract RGBColor Update(double tick_time);
    }
}
