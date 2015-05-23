using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using HexLight.Engine;
using HexLight.Colour;

namespace HexLight.Engine.Anim
{
    public enum Mode { Cycle };

    [EngineName("Anim")]
    public partial class AnimEngine : HexEngine
    {
        #region Properties

        public Mode Mode { get; set; }

        public double Speed { get; set; }

        #endregion

        public AnimEngine()
        {
            Mode = Mode.Cycle;
            Speed = 0.1;
        }

        public override UserControl GetControlPage()
        {
            var page = new ControlPage();

            page.DataContext = this;

            // Allow parent to define width/height
            page.Width = double.NaN;
            page.Height = double.NaN;

            return page;
        }

        public override void Enable()
        {
            
        }

        public override void Disable()
        {
            
        }

        public override RGBColor Update(double tickTime)
        {
            return CycleHueUpdate(tickTime);
        }
    }
}
