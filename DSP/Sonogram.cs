using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexLight.DSP
{
    class Sonogram
    {
        private int bufferSize;
        public float[][] Lines;
        private int lineIndex;

        public Sonogram(int bufferSize)
        {
            this.bufferSize = bufferSize;

            Lines = new float[bufferSize][];
            lineIndex = 0;
        }

        public void AddLine(float[] fft)
        {
            Lines[lineIndex++] = fft.ToArray();

            if (lineIndex == bufferSize)
                lineIndex = 0;
        }
    }
}
