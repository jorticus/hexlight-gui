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

namespace HexLight.Engine.Rowdz
{
    [EngineName("Rowdz")]
    public class RowdzEngine : HexEngine
    {
        public Rowdz.ViewModel viewModel;

        private bool enabled = false;

        private int fftWidth = 256;
        private DSP.WindowingFunction fftWindowingFunction = DSP.WindowingFunction.Boxcar;

        private AudioCapture audioCapture;
        private DSP.FastFourierTransform fft;
        //private DSP.BeatDetector beatDetector;
        //private DSP.Sonogram sonogram;
        private DSP.IntensityDetector intensityDetector;

        private float audioLevel = 0.0f;
        //private bool audioLoaded = false;

        private float[] leftch;
        private float[] rightch;
        private static Mutex bufferMutex = new Mutex();


        public RowdzEngine()
        {
            viewModel = new Rowdz.ViewModel();
        }

        public override UserControl GetControlPage()
        {
            var page = new ControlPage();

            page.DataContext = viewModel;

            // Allow parent to define width/height
            page.Width = double.NaN;
            page.Height = double.NaN;

            return page;
        }

        public override void Enable()
        {
            if (enabled)
                return;

            // Fok I'm so r0wdy

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
            if (!enabled)
                return;

            audioCapture.Stop();
            enabled = false;
        }

        public override RGBColor Update(double tick_time)
        {
            return Update();
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
            float sensitivity = (float)viewModel.Sensitivity;
            float decayRate = (float)viewModel.DecayRate; 

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
                intensityDetector.Decay = decayRate;
                intensityDetector.ProcessFFT(fft.Bands);
            }
            finally
            {
                bufferMutex.ReleaseMutex();
            }
        }

    }
}
