using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;

namespace HexLight.Control
{
    public class SimpleSerialControllerSettings : ControllerSettings
    {
        private int baud = 9600;
        private string port = "COM1";

        public int Baud
        {
            get { return baud; }
            set { baud = value; ConnChanged("Baud"); }
        }
        public string Port
        {
            get { return port; }
            set { port = value; ConnChanged("Port"); }
        }

        public override UserControl GetSettingsPage()
        {
            return new SerialSettingsPage();
        }
    }

    [ControllerName("Arduino")]
    [ControllerSettingsType(typeof(SimpleSerialControllerSettings))]
    public class SimpleSerialController : RGBController
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

        /// <summary>
        /// Parameterless constructor, use default settings
        /// </summary>
        public SimpleSerialController()
        {
            Settings = new SimpleSerialControllerSettings();
            Initialize();
        }

        public SimpleSerialController(ControllerSettings settings)
        {
            Settings = settings;
            Initialize();
        }

        protected void Initialize()
        {
            var settings = (Settings as SimpleSerialControllerSettings);
            this.port = settings.Port;
            this.baud = settings.Baud;
            serial = new SerialPort(port, baud);
            serial.Open();
            this.Connected = true;
            this.brightness = 1.0f;
        }

        public override void Dispose()
        {
            this.Connected = false;
            //Color = Colors.Black;
            serial.Close();
        }
    }
}
