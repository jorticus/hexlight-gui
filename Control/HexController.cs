using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGB.Util;
using RGB.Util.ColorTypes;

namespace RGB.Control
{
    public class HexController : RGBController, IDisposable
    {
        private const bool apply_cie = false;

        private RGBColor color;
        private float brightness;
        private string port;
        private int baud;
        private SerialPort serial;

        public override RGBColor Color
        {
            get { return color; }
            set
            {
                color = value;
                Update();
            }
        }

        public override float Brightness
        {
            get { return brightness; }
            set
            {
                brightness = value;
                Update();
            }
        }

        private float limit(float val) {
            if (val < 0.0f) return 0.0f;
            if (val > 1.0f) return 1.0f;
            return val;
        }

        private void Update()
        {
            RGBColor rgb = this.color;//= CIE1931.CorrectRGB(this.color);

            var rbytes = BitConverter.GetBytes((float)limit(rgb.r));
            var gbytes = BitConverter.GetBytes((float)limit(rgb.g));
            var bbytes = BitConverter.GetBytes((float)limit(rgb.b));
            var wbytes = BitConverter.GetBytes(0.0f);               // White channel
            var lbytes = BitConverter.GetBytes((float)limit(brightness));         // Luminance


            List<byte> packet = new List<byte>();
            packet.Add((byte)'H');
            packet.Add(0); //padding
            packet.Add(0); //padding
            packet.Add(0); //padding
            packet.AddRange(rbytes);
            packet.AddRange(gbytes);
            packet.AddRange(bbytes);
            packet.AddRange(wbytes);
            packet.AddRange(lbytes);

            serial.Write(packet.ToArray(), 0, packet.Count);
        }

        public HexController(string port, int baud = 9600)
        {
            this.port = port;
            this.baud = baud;
            serial = new SerialPort(port, baud);
            serial.Open();
            this.Connected = true;
            this.brightness = 1.0f;
        }

        public void Dispose()
        {
            this.Connected = false;
            serial.Close();
        }
    }
}
