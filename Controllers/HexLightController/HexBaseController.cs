using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;
using HexLight.Util;

namespace HexLight.Control
{
    public abstract class HexBaseController : RGBController
    {
        private const int RETRIES = 3;

        #region Protocol Command Defs

        private const byte CMD_TEST = 0x00;
        private const byte CMD_POWER_ON = 0x01;
        private const byte CMD_POWER_OFF = 0x02;

        private const byte CMD_ACK = 0xFF;

        #endregion

        #region Protocol Interface

        /// <summary>
        /// Read a complete frame from the serial device.
        /// </summary>
        protected abstract byte[] ReceiveFrame();

        /// <summary>
        /// Abstract method to send a raw packet to the device
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="payload">Raw payload data (optional)</param>
        protected abstract void SendPacket(byte command, byte[] payload = null);

        /// <summary>
        /// Abstract method for reading the reply returned after sending a packet
        /// </summary>
        /// <remarks>
        /// Waits for a reply from the device, after sending a command.
        /// If the command is not what was expected, an exception is raised.
        /// DO NOT USE DIRECTLY - prefer TransferPacket instead.
        /// </remarks>
        /// <param name="expected_command">The command that is expected</param>
        /// <returns>Raw response data</returns>
        protected abstract byte[] ReadReply(byte expected_command);

        #endregion

        #region Protocol Transfer

        /// <summary>
        /// Send a command with an optional payload and optional response.
        /// </summary>
        /// <remarks>
        /// Waits until a reply is received.
        /// Will retry transmission a few times if the packet transfer fails.
        /// DO NOT USE DIRECTLY - prefer TransferPacket instead.
        /// </remarks>
        /// <exception cref="HLDCProtocolException">Corrupted/Lost Packet Data</exception>
        /// <exception cref="ControllerConnectionException">Fatal unrecoverable error - device has been disconnected for safety</exception>
        /// <param name="command">The command</param>
        /// <param name="payload">Optional raw payload</param>
        /// <returns>The raw data received</returns>
        protected byte[] TransferPacket(byte command, byte[] payload = null)
        {
            int i = 0;
            while (true)
            {
                try
                {
                    SendPacket(command, payload);
                    return ReadReply(command);
                }
                catch (HLDCProtocolException)
                {
                    if (i++ == RETRIES)
                        throw;

                    continue;
                }
            }
        }

        /// <summary>
        /// Wrapper for TransferPacket(byte command, byte[] payload)
        /// </summary>
        /// 
        /// <typeparam name="T">Struct type for payload</typeparam>
        /// <typeparam name="R">Struct type for response</typeparam>
        /// <param name="command">Command to send</param>
        /// <param name="payload">Payload struct</param>
        /// <returns>Response struct</returns>
        protected R TransferPacket<T, R>(byte command, T payload)
        {
            byte[] payload_bytes = StructInterop.StructToByteArray<T>(payload);
            byte[] response_bytes = TransferPacket(command, payload_bytes);
            return StructInterop.ByteArrayToStruct<R>(response_bytes);
        }

        /// <summary>
        /// Wrapper for TransferPacket(byte command, byte[] payload)
        /// </summary>
        /// <remarks>
        /// Doesn't expect a response. Any response data is discarded,
        /// but will still block until a response is received.
        /// </remarks>
        /// <typeparam name="T">Struct type for payload</typeparam>
        /// <param name="command">Command to send</param>
        /// <param name="payload">Payload struct</param>
        protected void TransferPacket<T>(byte command, T payload)
        {
            byte[] payload_bytes = StructInterop.StructToByteArray<T>(payload);
            TransferPacket(command, payload_bytes);
        }

        /// <summary>
        /// Wrapper for TransferPacket(byte command, byte[] payload)
        /// </summary>
        /// <remarks>
        /// Command doesn't expect a payload, but returns a response.
        /// </remarks>
        /// <typeparam name="R">Struct type for response</typeparam>
        /// <param name="command">Command to send</param>
        /// <returns>Response struct</returns>
        protected R TransferPacket<R>(byte command)
        {
            byte[] response_bytes = TransferPacket(command);
            return StructInterop.ByteArrayToStruct<R>(response_bytes);
        }

        #endregion

        #region Protocol Implementation

        public void PowerOn()
        {
            TransferPacket(CMD_POWER_ON);
        }

        public void PowerOff()
        {
            TransferPacket(CMD_POWER_OFF);
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
