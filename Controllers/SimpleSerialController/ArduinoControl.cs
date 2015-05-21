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

            // Simple packet format - the char 'X' followed by 3 bytes, each 0-255.
            byte[] packet = { (byte)'X', value.Rb, value.Gb, value.Bb };

            try
            {
                serial.Write(packet, 0, 4);
            }
            catch (Exception ex)
            {
                // If there are any issues with the connection, disconnect.
                Disconnect();
                throw new ControllerConnectionException("Connection to controller lost", innerException:ex);
            }
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
            this.brightness = 1.0f;
        }

        public override void Connect()
        {
            try
            {
                if (!serial.IsOpen)
                    serial.Open();
            }
            catch (Exception ex)
            {
                throw new ControllerConnectionException("Could not connect to Arduino controller", innerException: ex);
            }
            NotifyConnect();
        }

        public override void Disconnect()
        {
            if (serial.IsOpen)
                serial.Close();
            NotifyDisconnect();
        }

        public override void Dispose()
        {
            Disconnect();
        }
    }
}
