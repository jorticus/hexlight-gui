using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
using System.Threading;

namespace HexLight.Engine.Util
{
    public delegate void ProcessAudioEvent(short[] buffer);

    public class AudioCaptureException : Exception
    {
        public AudioCaptureException(string message) : base(message) { }
    }

    /// <summary>
    /// Encapsulates audio capture
    /// </summary>
    public class AudioCapture
    {
        // Callback for processing captured audio data
        public ProcessAudioEvent onProcessAudio { get; set; }

        // Specifies the amount of audio data to capture in equivalent milliseconds
        public const int latency = 50; //ms

        public float SampleRate { get; private set; }
        public int NumSamples { get { return numberOfSamples; } }
        public WaveFormat WaveFormat { get { return waveFormat; } }

        private DirectSoundCapture directSoundCapture;
        private CaptureBufferDescription captureBufferDesc;
        private CaptureBuffer captureBuffer;
        private WaveFormat waveFormat;

        private Thread thread;

        private int numberOfSamples;
        private int bufferSize;

        private WaitHandle[] fullEvent;

        public AudioCapture()
        {
            try
            {
                directSoundCapture = new DirectSoundCapture();
            }
            catch
            {
                throw new AudioCaptureException("Could not open recording device");
            }
            //var directSoundCaps = directSoundCapture.Capabilities;

            // Default 44.1kHz 16-bit stereo PCM
            waveFormat = new WaveFormat();

            // Set the buffer size.
            // Note that the buffer position will roll over to 0 when the buffer fills up,
            // so set the notification position's offset to one less than the buffer size.
            bufferSize = waveFormat.ConvertLatencyToByteSize(latency);
            numberOfSamples = bufferSize / waveFormat.BlockAlign;

            // Create audio capture buffer
            captureBufferDesc = new CaptureBufferDescription();
            captureBufferDesc.Format = waveFormat;
            captureBufferDesc.BufferBytes = bufferSize;
            captureBuffer = new CaptureBuffer(directSoundCapture, captureBufferDesc);

            // Wait events allow the thread to wait asynchronously for the buffer to fill
            var evt = new AutoResetEvent(false);
            fullEvent = new WaitHandle[] { evt };

            // Notify the thread when the buffer is full
            var nf = new NotificationPosition();
            nf.Offset = bufferSize-1;
            nf.WaitHandle = fullEvent[0];
            var nfs = new NotificationPosition[] { nf };
            captureBuffer.SetNotificationPositions(nfs);

            // Start the processing thread
            thread = new Thread(new ThreadStart(Process));
            thread.IsBackground = true; // Allow application to exit
            thread.Start();
        }

        public void Dispose()
        {
            Stop();
        }


        public void Start()
        {
            if (onProcessAudio == null)
                throw new Exception("onProcessAudio must be assigned before starting conversion");

            captureBuffer.Start(true);
        }

        public void Stop()
        {
            captureBuffer.Stop();
        }

        private void Process()
        {
            //lock (this.thread)
            while(true)
            {
                // Wait for the buffer to fill up (no timeout)
                WaitHandle.WaitAny(this.fullEvent, -1, false);

                // Capture audio data
                var data = new short[numberOfSamples * 2]; // Stereo
                captureBuffer.Read(data, captureBuffer.CurrentRealPosition, LockFlags.FromWriteCursor);

                onProcessAudio(data);

                /*int num = captureBuffer.CurrentCapturePosition - captureBuffer.CurrentRealPosition;

                if (num > 0)
                {
                    onProcessAudio(data, num);
                }*/
            }
        }

    }
}
