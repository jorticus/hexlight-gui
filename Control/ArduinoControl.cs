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
    public class ArduinoController : RGBController, IDisposable
    {
        private const bool apply_cie = true;

        private RGBColor color;
        private string port;
        private int baud;
        private SerialPort serial;

        public override RGBColor Color
        {
            get { return color; }
            set
            {
                color = value;

                RGBColor corrected = (apply_cie) ? CIE1931.CorrectRGB(value) : value;
                byte[] packet = { (byte)'X', corrected.Rb, corrected.Gb, corrected.Bb };
                serial.Write(packet, 0, 4);
            }
        }

        public ArduinoController(string port, int baud = 9600)
        {
            this.port = port;
            this.baud = baud;
            serial = new SerialPort(port, baud);
            serial.Open();
            this.Connected = true;
        }

        public void Dispose()
        {
            this.Connected = false;
            serial.Close();
        }
    }
}
