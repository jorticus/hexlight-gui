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
    public abstract class HexBaseController : RGBController
    {
        #region Protocol Command Defs

        private const byte CMD_TEST = 0x00;
        private const byte CMD_POWER_ON = 0x01;
        private const byte CMD_POWER_OFF = 0x02;

        private const byte CMD_ACK = 0xFF;

        #endregion

        #region Protocol Handlers

        /// <summary>
        /// Read a complete frame from the serial device.
        /// </summary>
        protected abstract byte[] ReceiveFrame();

        /// <summary>
        /// Send a command to the device
        /// </summary>
        /// <typeparam name="T">The struct to use for parameters</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="payload">Command parameter data to send</param>
        protected abstract void SendPacket<T>(byte command, T payload);
        protected abstract void SendPacket(byte command, byte[] payload = null);

        /// <summary>
        /// Wait for a reply from the device, after sending a command.
        /// If the command is not what was expected, an exception is raised.
        /// </summary>
        /// <param name="expected_command">The command that is expected</param>
        /// <returns>Raw payload bytes</returns>
        protected abstract byte[] ReadReply(byte expected_command);
        protected abstract T ReadReply<T>(byte expected_command);

        #endregion

        #region Protocol Implementation

        public void PowerOn()
        {
            SendPacket(CMD_POWER_ON);
            ReadReply(CMD_POWER_ON);
        }

        public void PowerOff()
        {
            SendPacket(CMD_POWER_OFF);
            ReadReply(CMD_POWER_OFF);
        }

        #endregion

        #region Utility Functions

        protected float limit(float val)
        {
            if (val < 0.0f) return 0.0f;
            if (val > 1.0f) return 1.0f;
            return val;
        }

        protected Int16 FloatToInt16(float x)
        {
            if (x < 0.0f) return 0;
            if (x > 1.0f) return Int16.MaxValue;
            return (Int16)(x * (float)Int16.MaxValue);
        }

        #endregion

        public HexBaseController()
        {

        }
    }
}
