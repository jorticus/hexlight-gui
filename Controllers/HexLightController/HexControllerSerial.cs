using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using HexLight.Colour;

namespace HexLight.Control
{
    public class HexControllerSerialSettings : ControllerSettings
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

        /// <summary>
        /// Returns the XAML form to use for the settings model
        /// </summary>
        public override UserControl GetSettingsPage()
        {
            return new SerialSettingsPage();
        }
    }

    [ControllerName("HexLight Serial")]
    [ControllerSettingsType(typeof(HexControllerSerialSettings))]
    public class HexControllerSerial : HexController, IDisposable
    {
        private string port;
        private int baud;
        private SerialPort serial;


        #region Protocol Handlers

        /// <summary>
        /// Read a complete frame from the serial device.
        /// </summary>
        protected override byte[] ReceiveFrame()
        {
            HLDCFramer framer = new HLDCFramer();
            while (true)
            {

                serial.WriteTimeout = 1000;
                int b = serial.ReadByte();

                bool finished = framer.ProcessByte((byte)b);

                if ((b < 0) || (finished && framer.FrameBytes != null))
                    break;
            }
            return framer.FrameBytes;
        }

        /// <summary>
        /// Send a command to the device
        /// </summary>
        /// <typeparam name="T">The struct to use for parameters</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="payload">Command parameter data to send</param>
        protected override void SendPacket<T>(byte command, T payload)
        {
            byte[] packet = HLDCProtocol.CreatePacket<T>(command, payload);
            serial.Write(packet, 0, packet.Length);
        }

        protected override void SendPacket(byte command, byte[] payload = null)
        {
            byte[] packet = HLDCProtocol.CreatePacket(command, payload);
            serial.Write(packet, 0, packet.Length);
        }

        /// <summary>
        /// Wait for a reply from the device, after sending a command.
        /// If the command is not what was expected, an exception is raised.
        /// </summary>
        /// <param name="expected_command">The command that is expected</param>
        /// <returns>Raw payload bytes</returns>
        protected override byte[] ReadReply(byte expected_command)
        {
            byte[] response = ReceiveFrame();
            byte command;
            byte[] payload;
            HLDCProtocol.ParsePacket(response, out command, out payload);

            if (command != expected_command)
                throw new Exception("HLDC Protocol Error - Invalid reply");

            return payload;
        }

        protected override T ReadReply<T>(byte expected_command)
        {
            byte[] response = ReceiveFrame();
            byte command;
            T payload;
            HLDCProtocol.ParsePacket<T>(response, out command, out payload);

            if (command != expected_command)
                throw new Exception("HLDC Protocol Error - Invalid reply");

            return payload;
        }

        #endregion

        /// <summary>
        /// Open access to the device
        /// </summary>
        protected override void Open()
        {
            serial.Open();
        }

        /// <summary>
        /// Close access to the device
        /// </summary>
        protected override void Close()
        {
            serial.Close();
        }

        /// <summary>
        /// Parameterless constructor, use default settings
        /// </summary>
        public HexControllerSerial()
        {
            Settings = new HexControllerHIDSettings();
            Initialize();
        }

        public HexControllerSerial(ControllerSettings settings)
        {
            Settings = settings;
            Initialize();
        }

        protected void Initialize()
        {
            var settings = (Settings as HexControllerSerialSettings);
            this.port = settings.Port;
            this.baud = settings.Baud;
            serial = new SerialPort(port, baud);
            Open();
            this.Connected = true;
        }

        public override void Dispose()
        {
            Close();
            this.Connected = false;
        }
    }
}
