using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HexLight.Engines.Util;
using System.Drawing;
using HexLight.Colour;

namespace HexLight.DSP
{

    public class IntensityBand
    {
        public float PreIntensity { get; private set; } // Before weighting
        public float Intensity { get; private set; }

        public float Frequency { get; set; }    // Normalized Frequency (0.0 to 1.0)
        public float Bandwidth { get; set; }    // Normalized Bandwidth (0.0 to 1.0)

        public float Weight { get; set; }
        public float Threshold { get; set; }
        public float Limit { get; set; }

        private CircularBuffer<float> Buffer;

        public IntensityBand(float frequency, float bandwidth, int bufferSize)
        {
            Intensity = 0.0f;

            Frequency = frequency;
            Bandwidth = bandwidth;

            Weight = 1.0f;
            Threshold = 0.0f;
            Limit = 1.0f;

            Buffer = new CircularBuffer<float>(bufferSize);
        }

        private float ValueThreshold(float value, float threshold)
        {
            if (value > threshold)
                return (value - threshold) * (1 / (1.0f - threshold));
            else
                return 0.0f;
        }

        public void ProcessFFT(float[] bands)
        {
            // Calculate which band the frequency is in
            int idx = (int)(bands.Length * Frequency);
            int bw = (int)(bands.Length * Bandwidth);
            //TODO: bandwidth

            int i1 = idx - bw/2;
            int i2 = idx + bw/2;
            if (i1 < 0) i1 = 0;
            if (i2 >= bands.Length) i2 = bands.Length-1;
            i2++;

            float value = 0.0f;
            for (int i = i1; i < i2; i++)
            {
                value += bands[idx];
            }
            value /= (float)bw;

            // Add sample to circular buffer
            Buffer.Add(ValueThreshold(value, Threshold));

            // Average
            float average = CircularBuffer<float>.Average(Buffer); //TODO: Uncomment
            PreIntensity = average;

            // Weight & Limit
            Intensity = Math.Min(average * Weight, Limit);

        }
    }

    public class IntensityDetector
    {
        // Calculation Result
        public float Intensity { get; private set; }

        // Settings
        public float Decay { get; set; }
        public List<IntensityBand> IntensityBands { get; set; }

        private const int BufferSize = 4;

        public RGBColor Colour
        {
            get
            {
                if (Intensity < 0.5f)
                    return RGBColor.Interpolate(Color.CornflowerBlue, Color.LimeGreen, Intensity * 2.0f);
                else
                    return RGBColor.Interpolate(Color.LimeGreen, Color.Red, (Intensity - 0.5f) * 2.0f);
            }
        }

        public IntensityDetector()
        {
            Decay = 0.01f;

            IntensityBands = new List<IntensityBand>();
            IntensityBands.Add(new IntensityBand(0.33f, 0.167f, BufferSize)
            {
                Threshold = 0.2f,
                Weight = 1.0f,
                Limit = 0.33f
            });
            IntensityBands.Add(new IntensityBand(0.5f, 0.167f, BufferSize)
            {
                Threshold = 0.2f,
                Weight = 2.0f,
                Limit = 1.0f
            });
        }


        
        public void ProcessFFT(float[] bands)
        {
            // Attack
            foreach (var intensityBand in IntensityBands)
            {
                intensityBand.ProcessFFT(bands);
                Intensity = Math.Max(Intensity, intensityBand.Intensity);
            }

            // Decay
            Intensity -= Decay;

            // Limit
            if (Intensity > 1.0f) Intensity = 1.0f;
            if (Intensity < 0.0f) Intensity = 0.0f;
        }
    }
}
