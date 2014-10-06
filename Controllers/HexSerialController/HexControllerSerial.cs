using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;

namespace HexLight.Control
{
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


        public HexControllerSerial(string port, int baud = 9600)
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
