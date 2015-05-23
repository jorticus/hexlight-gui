using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using HexLight.Engine;
using HexLight.Colour;
using HexLight.Engine.Util;

namespace HexLight.Engine
{
    public class RowdzEngine : HexEngine
    {
        private bool enabled = false;

        private int fftWidth = 256;
        private DSP.WindowingFunction fftWindowingFunction = DSP.WindowingFunction.Boxcar;

        private AudioCapture audioCapture;
        private DSP.FastFourierTransform fft;
        //private DSP.BeatDetector beatDetector;
        //private DSP.Sonogram sonogram;
        private DSP.IntensityDetector intensityDetector;

        private float audioLevel = 0.0f;
        public float sensitivity = 1.0f;
        //private bool audioLoaded = false;
        public float intensityDecayRate = 0.01f;

        private float[] leftch;
        private float[] rightch;
        private static Mutex bufferMutex = new Mutex();


        public RowdzEngine()
        {
            // Yer!
        }

        public override UserControl GetControlPage()
        {
            throw new NotImplementedException();
        }

        public override void Enable()
        {
            // Open audio device
            try
            {
                audioCapture = new AudioCapture();
                audioCapture.onProcessAudio += this.AudioBufferFull;
                //audioLoaded = true;

                leftch = new float[audioCapture.NumSamples];
                rightch = new float[audioCapture.NumSamples];
                fft = new DSP.FastFourierTransform(fftWidth, audioCapture.SampleRate);
                fft.NumBands = 6;

                //beatDetector = new DSP.BeatDetector(audioCapture.WaveFormat, 1000, audioCapture.NumSamples);

                //sonogram = new DSP.Sonogram(d2dCanvas.Height);

                intensityDetector = new DSP.IntensityDetector();

                audioCapture.Start();
            }
            catch (AudioCaptureException ex)
            {
  
                throw new Exception("Could not open audio device", ex);
            }

            enabled = true;
        }

        public override void Disable()
        {
            audioCapture.Stop();
            enabled = false;
        }

        public override RGBColor Update(double tick_time)
        {
            throw new NotImplementedException();
        }

        public RGBColor Update()
        {
            if (enabled)
            {
                ProcessAudio();
                return intensityDetector.Colour;
            }
            return new RGBColor(0, 0, 0);
        }

        private void ProcessAudio()
        {
            //fftWF = (float)tbParam1.Value / 10.0f;
            //fftWidth = fmConfigureFFT.WindowSize;
            //fftWindowingFunction = fmConfigureFFT.WindowingFunction;

            //bufferMutex.WaitOne();
            try
            {
                // Audio Level
                float max = 0;
                for (int i = 0; i < leftch.Length; i++) // Only process one channel
                {
                    float absvalue = Math.Abs(leftch[i]);
                    if (absvalue > max) max = absvalue;
                }
                audioLevel = max;
                if (audioLevel > 1.0f) audioLevel = 1.0f;
            }
            finally
            {
                //bufferMutex.ReleaseMutex();
            }
        }

        // UNSAFE THREAD CALL
        private void AudioBufferFull(short[] buffer)
        {
            bufferMutex.WaitOne();
            try
            {
                int j = 0;
                for (int i = 0; i < buffer.Length; i += 2)
                {
                    leftch[j++] = ((buffer[i] / (float)short.MaxValue) * sensitivity);
                    //leftch[j++] = (float)Math.Sin(x); x += 5;
                }
                j = 0;
                /*for (int i = 1; i < buffer.Length; i += 2)
                {
                    rightch[j++] = ((buffer[i] / (float)short.MaxValue) * sensitivity);
                }*/

                // FFT
                //fft.param1 = fftWF;
                fft.Width = fftWidth;
                fft.WindowingFunction = fftWindowingFunction;
                fft.FrequencyScale = DSP.FrequencyScale.Linear;
                fft.AmplitudeUnits = DSP.AmplitudeUnits.Amplitude;
                //fft.FrequencyScale = fmConfigureFFT.FrequencyScale;
                //fft.AmplitudeUnits = fmConfigureFFT.AmplitudeUnits;
                //fft.DecayRate = fmConfigureFFT.DecayRate;
                fft.Calculate(leftch);
                //fft.NumBands = fmConfigureFFT.NumBands;
                //beatDetector.ProcessAudio(leftch);

                //sonogram.AddLine(fft.Buckets);
                //sonogram.AddLine(fft.Bands);

                //intensityDetector.Threshold = AudioEditor.threshold;
                intensityDetector.Decay = intensityDecayRate;
                intensityDetector.ProcessFFT(fft.Bands);
            }
            finally
            {
                bufferMutex.ReleaseMutex();
            }
        }

    }
}
