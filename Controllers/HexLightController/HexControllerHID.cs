﻿using System;
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
            byte[] buffer = new byte[COMMAND_PACKET_SIZE];
            uint r = rx.Read(buffer, COMMAND_PACKET_SIZE);
            if (r != COMMAND_PACKET_SIZE)
                throw new WinAPI.WinApiFileException("Received data invalid - length mismatch");

            // Throw away the first byte (Windows Reserved)
            buffer = buffer.Skip(1).ToArray();

            return buffer;

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
        //protected override void SendPacket<T>(byte command, T payload)
        //{
        //    if (!Connected)
        //        throw new Exception("Controller is not connected");

        //    try
        //    {
        //        byte[] packet = HexProtocol.CreatePacket<T>(command, payload);
        //        WritePacket(packet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Disconnect();
        //        throw new ControllerConnectionException("Connection to controller lost", innerException:ex);
        //    }
        //}

        protected override void SendPacket(byte command, byte[] payload = null)
        {
            if (!Connected)
                throw new Exception("Controller is not connected");

            try
            {
                byte[] packet = HexProtocol.CreatePacket(command, payload);
                WritePacket(packet);
            }
            catch (Exception ex)
            {
                Disconnect();
                throw new ControllerConnectionException("Connection to controller lost", innerException:ex);
            }
        }

        /// <summary>
        /// Wait for a reply from the device, after sending a command.
        /// If the command is not what was expected, an exception is raised.
        /// </summary>
        /// <param name="expected_command">The command that is expected</param>
        /// <returns>Raw payload bytes</returns>
        protected override byte[] ReadReply(byte expected_command)
        {
            try {
                byte[] response = ReceiveFrame();
                byte command;
                byte[] payload;
                HexProtocol.ParsePacket(response, out command, out payload);

                if (command != expected_command)
                    throw new HLDCProtocolException("Unexpected or invalid reply received");

                return payload;
            }
            catch (Exception ex)
            {
                Disconnect();
                throw new ControllerConnectionException("Connection to controller lost", innerException: ex);
            }
        }

        //protected override T ReadReply<T>(byte expected_command)
        //{
        //    try {
        //        byte[] response = ReceiveFrame();
        //        byte command;
        //        T payload;
        //        HexProtocol.ParsePacket<T>(response, out command, out payload);

        //        if (command != expected_command)
        //            throw new HLDCProtocolException("Unexpected or invalid reply received");

        //        return payload;
        //    }
        //    catch (Exception ex)
        //    {
        //        Disconnect();
        //        throw new ControllerConnectionException("Connection to controller lost", innerException: ex);
        //    }
        //}

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
        }

        public override void Connect()
        {
            if (!Connected)
            {
                this.device = new HidDevice(deviceID);

                // Look up the VID/PID in the connected devices registry
                try
                {
                    this.device.Scan();
                }
                catch (HidDeviceException ex)
                {
                    throw new ControllerConnectionException("Could not find USB controller - is it plugged in?", innerException: ex);
                }

                // Open file handles for communication
                try
                {
                    tx = device.GetWriteFile();
                    rx = device.GetReadFile();
                }
                catch (Exception ex)
                {
                    throw new ControllerConnectionException("Could not connect to USB controller", innerException:ex);
                }


                // required when running outside visual studio for some reason???
                //SendPacket(0x00);
            }
            NotifyConnect();
        }

        public override void Disconnect()
        {
            if (Connected)
            {
                tx.Close();
                rx.Close();
                this.device = null;
            }
            NotifyDisconnect();
        }

        public override void Dispose()
        {
            Disconnect();
        }
    }
}
