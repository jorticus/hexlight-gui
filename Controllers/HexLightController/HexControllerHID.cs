using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexLight.Util;
using HexLight.Colour;
using HexLight.HID;
using System.Configuration;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Windows.Controls;
using System.Windows.Media;

namespace HexLight.Control
{
    
    public class HexControllerHIDSettings : ControllerSettings
    {
        private string deviceId = "VID_04D8&PID_1E00";
        private bool usbAudioEnabled = false;

        public string DeviceID
        {
            get { return deviceId; }
            set { deviceId = value; ConnChanged("DeviceID"); }
        }

        public bool UsbAudioEnabled
        {
            get { return usbAudioEnabled; }
            set {
                // Update the controller (Immediate effect)
                var controller = (this.Controller as HexControllerHID);
                if (controller != null)
                    controller.EnableUsbAudio(value);

                usbAudioEnabled = value;
                PropChanged("UsbAudioEnabled");
            }
        }

        /// <summary>
        /// Returns the XAML form to use for the settings model
        /// </summary>
        public override UserControl GetSettingsPage()
        {
            return new HIDSettingsPage();
        }
    }

    [ControllerName("HexLight USB-HID")]
    [ControllerSettingsType(typeof(HexControllerHIDSettings))]
    public class HexControllerHID : HexController, IDisposable
    {
        //[XmlAttribute]
        private string deviceID;
        private HidDevice device;

        private WinAPI.WinApiFile tx;
        private WinAPI.WinApiFile rx;

        #region Protocol Structs

        private const int COMMAND_PACKET_SIZE = 65;

        /*[StructLayout(LayoutKind.Sequential, Pack = 1, Size = COMMAND_PACKET_SIZE)]
        public struct GenericHidPacket
        {
            public byte WindowsReserved;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = COMMAND_PACKET_SIZE-1)]
            public byte[] Data;
        }*/

        #endregion

        #region Protocol Handlers

        /// <summary>
        /// Read a complete frame from the serial device.
        /// </summary>
        protected override byte[] ReceiveFrame()
        {
            HLDCFramer framer = new HLDCFramer();
            while (true)
            {
                byte[] buffer = new byte[COMMAND_PACKET_SIZE];
                rx.Read(buffer, COMMAND_PACKET_SIZE);

                foreach (var b in buffer)
                {
                    bool finished = framer.ProcessByte(b);
                    if (finished && framer.FrameBytes != null)
                        return framer.FrameBytes;
                }
            }
        }

        private void WritePacket(byte[] packet)
        {
            List<byte> data = new List<byte>() { 0 };
            data.AddRange(packet);

            int padding = COMMAND_PACKET_SIZE - data.Count;
            for (int i = 0; i < padding; i++)
                data.Add(0);

            tx.Write(data.ToArray(), COMMAND_PACKET_SIZE);
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
            WritePacket(packet);
        }

        protected override void SendPacket(byte command, byte[] payload = null)
        {
            byte[] packet = HLDCProtocol.CreatePacket(command, payload);
            WritePacket(packet);
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
                throw new Exception("Invalid data received");

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

        /// <summary>
        /// Open access to the device
        /// </summary>
        protected override void Open()
        {
            tx = device.GetWriteFile();
            rx = device.GetReadFile();
        }

        /// <summary>
        /// Close access to the device
        /// </summary>
        protected override void Close()
        {
            tx.Close();
            rx.Close();
        }

        #endregion

        /// <summary>
        /// Parameterless constructor, use default settings
        /// </summary>
        public HexControllerHID()
        {
            Settings = new HexControllerHIDSettings();
            Initialize();
        }

        public HexControllerHID(ControllerSettings settings)
        {
            Settings = settings;
            Initialize();
        }

        protected void Initialize()
        {
            var settings = (Settings as HexControllerHIDSettings);
            this.deviceID = settings.DeviceID;
            this.device = new HidDevice(deviceID);
            this.device.Scan();

            Open();

            //this.EnableUsbAudio(settings.UsbAudioEnabled);

            // required when running outside visual studio for some reason???
            SendPacket(0x00);

            this.Connected = true;
        }

        public override void Dispose()
        {
            Color = Colors.Black;
            this.Connected = false;

            Close();
        }
    }
}
