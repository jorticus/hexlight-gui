using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexLight.DSP
{
    public abstract class Window
    {
        public virtual void Apply(ref float[] values) { }

        /// <summary>
        /// Boxcar window
        /// All data outside the array is truncated to zero.
        /// This has no actual effect on the array.
        /// </summary>
        public class Boxcar : Window
        {
            public override void Apply(ref float[] values) { } // Do nothing
        }
        /// <summary>
        /// Hamming window
        /// Applies a hamming window to the given data
        /// </summary>
        public class Hamming : Window
        {
            public override void Apply(ref float[] values)
            {
                int N = values.Length;
                for (int i = 0; i < N; i++)
                {
                    values[i] = values[i] * (float)(0.54 - 0.46 * Math.Cos(2.0 * Math.PI * (double)i / (double)N));
                }
            }
        }
    }

    public class Band
    {
        /*public float Re { get; private set; }
        public float Im { get; private set; }

        public float Amplitude { get; private set; }
        public float Energy { get; private set; }*/

        public float Amplitude { get; private set; }
        public float Frequency { get; private set; }

        public Band(float amplitude, float frequency)
        {
            Amplitude = amplitude;
            Frequency = frequency;
        }

        /*public Band(float real, float imag)
        {
            this.Re = real;
            this.Im = imag;

            Energy = real * real + imag * imag;
            Amplitude = (float)Math.Sqrt(Energy);
        }*/
    }


    public enum WindowingFunction { Boxcar, Hamming }
    public enum FrequencyScale { Linear, Logarithmic }
    public enum AmplitudeUnits { Amplitude, Energy }

    public class FastFourierTransform
    {
        public float[] Bands { get; private set; }
        public float[] Buckets { get; private set; }
        //public float[] AveragedBuckets { get; private set; }
        public float[] PersistentBuckets { get; private set; }

        public float MaximumFrequency { get; private set; }
        public int Width { get; set; }
        public float SampleRate { get; private set; }
        public float DecayRate { get; set; }
        public int NumBands { get; set; }

        public WindowingFunction WindowingFunction { get; set; }
        public FrequencyScale FrequencyScale { get; set; }
        public AmplitudeUnits AmplitudeUnits { get; set; }

        public float param1 { get; set; }

        public FastFourierTransform(int Width, float sampleRate)
        {
            this.Width = Width;
            this.SampleRate = sampleRate;

            Buckets = new float[Width];
            PersistentBuckets = new float[Width];
            Bands = new float[Width];
            param1 = 2.0f;
            DecayRate = 1.0f;

            WindowingFunction = WindowingFunction.Boxcar;
            FrequencyScale = FrequencyScale.Linear;
            AmplitudeUnits = AmplitudeUnits.Amplitude;
        }

        /*private uint ReverseBits(uint x)
        {
            //uses the SWAR algorithm for bit reversal of 16bits
            x = (((x & 0xaaaaaaaa) >> 1) | ((x & 0x55555555) << 1));
            x = (((x & 0xcccccccc) >> 2) | ((x & 0x33333333) << 2));
            x = (((x & 0xf0f0f0f0) >> 4) | ((x & 0x0f0f0f0f) << 4));	//8bits

            //uncomment for 9 bit reversal
            //x = (((x & 0xff00ff00) >> 8) | ((x & 0x00ff00ff) << 8));	//16bits
            //push us back down into 9 bits
            //return (x >> (16-9) & 0x000001ff;

            return x;
        }*/

        private void ApplyWindow(ref float[] samples)
        {
            Window window;
            switch (WindowingFunction)
            {
                case WindowingFunction.Hamming:
                    window = new Window.Hamming();
                    break;
                default:
                    window = new Window.Boxcar();
                    break;
            }
            window.Apply(ref samples);
        }

        public void Calculate(float[] samples)
        {
            //NOTE: size of samples array is assumed to be equal to numSamples.

            ApplyWindow(ref samples);


            int N = this.Width;

 	        int k, n;
		    float WN, wk, c, s;
		    float XR,XI;

		    WN = (float)(2*Math.PI) / (float)(N*param1); // Weighting factor

            double logN = 0;
            if (FrequencyScale == FrequencyScale.Logarithmic)
                logN = Math.Log10(N);

		    // Perform FFT (NOTE: I'm pretty sure this is actually a DFT, it's very slow.)
		    for (k=0; k<N; k++) {
			    XR = 0; XI = 0;

                if (FrequencyScale == FrequencyScale.Logarithmic)
                    wk = (float)Math.Pow(10.0, (double)k / (double)N * logN) * WN; // Logarithmic FFT
                else
                    wk = k * WN;  // Linear FFT

			    for (n=0; n<N; n++) {
                    float sample = samples[n];// *0.1f;
                    c = (float)Math.Cos(n * wk); s = (float)Math.Sin(n * wk);
				    XR = XR + sample * c;
                    XI = XI - sample * s;
			    }

                // Convert to real units
                float value = 0;
                switch (AmplitudeUnits) {
                    case AmplitudeUnits.Amplitude:
                        value = (float)Math.Sqrt(XR * XR + XI * XI); // Amplitude
                        break;
                    case AmplitudeUnits.Energy:
                        value = (float)(XR * XR + XI * XI); //Energy
                        break;
                }
                //float value = (float)Math.Abs(XR);
                //float value = 20.0f * (float)(Math.Log10(Math.Sqrt(XR * XR + XI * XI))); // dB


                // Weight the value with frequency (not sure why I need this, but it makes the amplitudes right)
                //value = (float)(value * Math.Pow(10.0, (double)k / N) / 1.0);
                value = value * (1.0f + (float)k / N);
                //value = value * (float)k / N * 10.0f;
                

                Buckets[k] = value;

                if (DecayRate >= 1.0f)
                {
                    PersistentBuckets[k] = Buckets[k];
                }
                else
                {
                    if (value > PersistentBuckets[k])
                        PersistentBuckets[k] = value;
                    PersistentBuckets[k] -= DecayRate;
                }
		    }

            Bands = GetBands(NumBands);               
        }

        public float[] GetBands(int numBands) {
            float[] bands = new float[numBands];

            int width = this.Width;
            //int width = Width * 10000 / 22050;

            int div = width / numBands; //+ some remainder
            int div2 = div / 2;

            int idx = 0;
            for (int i = 0; i < numBands; i++)
            {
                float band = 0.0f;
                for (int j = 0; j < div; j++)
                {
                    band += Buckets[idx++];
                }
                band /= (float)div;


                bands[i] = band;
                //int ifreq = i + div2;
                //bands[i] = new Band(band, ifreq);
            }

            return bands;
        }

    }
}
