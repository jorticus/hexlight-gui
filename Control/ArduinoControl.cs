using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexLight.Util;
using HexLight.Util.ColorTypes;

namespace HexLight.Control
{
    public class ArduinoController : RGBController, IDisposable
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

        private void Update()
        {
            RGBColor value = color * brightness;
            RGBColor corrected = (apply_cie) ? CIE1931.CorrectRGB(value) : value;
            byte[] packet = { (byte)'X', corrected.Rb, corrected.Gb, corrected.Bb };
            serial.Write(packet, 0, 4);
        }

        public ArduinoController(string port, int baud = 9600)
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
