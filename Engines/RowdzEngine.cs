using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;

namespace HexLight.Engines
{
    public class RowdzEngine
    {
        private bool enabled = false;

        public RowdzEngine()
        {
            // Yer!
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        public RGBColor Update()
        {
            if (enabled)
            {
                return new RGBColor(1, 0, 0);
            }
            return new RGBColor(0, 0, 0);
        }

    }
}
