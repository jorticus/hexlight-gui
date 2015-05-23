using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Multimedia;

namespace HexLight.DSP
{
    class BeatDetector
    {
        //public int HistoryLength { get; private set; }
        public float InstantaneousEnergy { get; private set; }
        public float AverageEnergy { get; private set; }
        //public float BeatEnergy { get { return (AverageEnergy - InstantaneousEnergy); } }
        public float BeatEnergy { get; private set; }
        public float[] Beats { get { return beats; } }
        public float[] FFT { get { return fft.Buckets; } }

        //private int bufferSize;         // samples
        private int historyLength;      // milliseconds
        private int historySize;        // buffers
        private int beatsSize;

        // Circular buffer
        private float[] history;
        private int historyIndex;

        private float[] beats;
        private int beatIndex;

        private FastFourierTransform fft;
      

        public BeatDetector(WaveFormat waveFormat, int historyLength, int bufferSize)
        {
            //this.bufferSize = bufferSize;
            this.historyLength = historyLength;

            // Number of history buffers needed to be equivalent to historyLength (milliseconds)
            historySize = waveFormat.ConvertLatencyToByteSize(historyLength) / bufferSize;
            beatsSize = historySize;

            history = new float[historySize];
            historyIndex = 0;

            beats = new float[beatsSize];
            beatIndex = 0;

            fft = new FastFourierTransform(beatsSize, 1);

            InstantaneousEnergy = 0.0f;
            AverageEnergy = 0.0f;
        }

        public void ProcessAudio(float[] audio)
        {
            // 1. Calculate average instantaneous energy
            float E = 0.0f;
            for (int i=0; i<audio.Length; i++) {
                float sample = audio[i];
                E += sample * sample; // Energy of sample
            }
            //E /= audio.Length;
            

            // 2. Add to history buffer
            history[historyIndex++] = E;
            if (historyIndex == history.Length)
                historyIndex = 0;

            // 3. Calculate average historical energy
            float H = 0.0f;
            for (int i=0; i<history.Length; i++) {
                H += history[i];
            }
            H /= (float)history.Length;


            InstantaneousEnergy = E;
            AverageEnergy = H;

            float B = Math.Abs(H - E);
            if (B > BeatEnergy)
                BeatEnergy = B;

            if (BeatEnergy > 1.0f) BeatEnergy = 1.0f;

            BeatEnergy -= 0.05f;

            // 4. Add to beat history buffer
            beats[beatIndex++] = BeatEnergy;
            if (beatIndex == beats.Length)
            {
                beatIndex = 0;
                //AnalyzeFrequencyContent();
            }
        }

        private void AnalyzeFrequencyContent()
        {
            fft.Calculate(beats);
        }
    }
}
