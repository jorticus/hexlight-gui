using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using HexLight.Util;

namespace HexLight.Control
{

    public class HLDCException : Exception {
        public HLDCException() : base() { }
        public HLDCException(string message) : base(message) { }
        public HLDCException(string message, Exception innerException) : base(message, innerException) { }

    }

    /// <summary>
    /// High-Level Data Link Control Protocol (Or at least a subset of it)
    /// http://en.wikipedia.org/wiki/High-Level_Data_Link_Control
    /// 
    /// Provides routines for parsing and creating HLDC packets. Takes care
    /// of framing packets so they can be synchronized at the other end.
    /// </summary>
    public class HLDCProtocol
    {
        #region Definitions

        public const byte HLDC_FRAME_DELIMITER = 0x7E;
        public const byte HLDC_ESCAPE = 0x7D;
        public const byte HLDC_ESCAPE_MASK = 0x20;

        public static readonly int MAX_PACKET_LEN = 64;
        public static readonly int HEADER_SIZE = Marshal.SizeOf(typeof(HeaderStruct));
        public static readonly int FOOTER_SIZE = Marshal.SizeOf(typeof(HeaderStruct));
        public static readonly int MIN_PACKET_LEN = HEADER_SIZE + FOOTER_SIZE;
        public static readonly int MAX_PAYLOAD_LEN = MAX_PACKET_LEN - MIN_PACKET_LEN;

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct HeaderStruct
        {
            public byte command;
            public byte length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FooterStruct
        {
            public UInt16 crc;
        }

        #endregion

        #region Utilities

        private static byte[] UnescapeBytes(byte[] buffer)
        {
            List<byte> resultBuffer = new List<byte>();
            int i = 0;
            while (i < buffer.Length)
            {
                if (buffer[i] == HLDC_ESCAPE)
                    resultBuffer.Add((byte)(buffer[++i] ^ HLDC_ESCAPE_MASK));
                else
                    resultBuffer.Add(buffer[i]);

                i++;
            }
            return resultBuffer.ToArray();
        }

        private static byte[] EscapeBytes(byte[] buffer)
        {
            List<byte> resultBuffer = new List<byte>();
            for (int i = 0; i < buffer.Length; i++)
            {
                byte data = buffer[i];
                if (data == HLDC_FRAME_DELIMITER || data == HLDC_ESCAPE)
                {
                    resultBuffer.Add(HLDC_ESCAPE);
                    resultBuffer.Add((byte)(data ^ HLDC_ESCAPE_MASK));
                }
                else
                    resultBuffer.Add(data);
            }
            return resultBuffer.ToArray();
        }

        public static UInt16 CalculateCrc(byte[] buffer)
        {
            return 0;
        }

        #endregion


        #region Send Packet

        public static byte[] CreatePacket(byte command, byte[] payload = null)
        {
            List<byte> packetBuilder = new List<byte>();

            if (payload != null && payload.Length > MAX_PAYLOAD_LEN)
                throw new Exception("HLDC payload too large");

            packetBuilder.Add(HLDC_FRAME_DELIMITER);

            // Build the packet header
            HeaderStruct header = new HeaderStruct()
            {
                command = command,
                length = (byte)((payload != null) ? payload.Length : 0)
            };
            packetBuilder.AddRange(EscapeBytes(StructInterop.StructToByteArray<HeaderStruct>(header)));

            // Build the packet payload
            UInt16 crc = 0;
            if (payload != null)
            {
                byte[] escapedPayload = EscapeBytes(payload);
                packetBuilder.AddRange(escapedPayload);
                crc = CalculateCrc(escapedPayload);
            }

            // BUild the packet footer
            FooterStruct footer = new FooterStruct()
            {
                crc = crc,
            };
            packetBuilder.AddRange(EscapeBytes(StructInterop.StructToByteArray<FooterStruct>(footer)));

            packetBuilder.Add(HLDC_FRAME_DELIMITER);

            if (packetBuilder.Count > MAX_PACKET_LEN)
                throw new Exception("HLDC packet too large");

            return packetBuilder.ToArray();
        }

        public static byte[] CreatePacket<T>(byte command, T struc)
        {
            byte[] payload = StructInterop.StructToByteArray<T>(struc);
            return CreatePacket(command, payload);
        }

        #endregion

        #region Receive Packet

        public static void ParsePacket(byte[] packet, out byte command, out byte[] payload)
        {
            // NOTE: Assumes packet has a complete HLDC frame (excluding the SOF/EOF bytes)
            // Use the HLDCFramer helper class to receive a complete HLDC frame

            var headerBytes = new ArraySegment<byte>(packet, 0, HEADER_SIZE).ToArray();
            HeaderStruct header = StructInterop.ByteArrayToStruct<HeaderStruct>(headerBytes);

            // Payload
            if (header.length > 0)
                payload = new ArraySegment<byte>(packet, HEADER_SIZE, header.length).ToArray();
            else
                payload = null;

            var footerBytes = new ArraySegment<byte>(packet, HEADER_SIZE + header.length, FOOTER_SIZE).ToArray();
            FooterStruct footer = StructInterop.ByteArrayToStruct<FooterStruct>(footerBytes);

            command = header.command;
        }

        public static void ParsePacket<T>(byte[] packet, out byte command, out T payload)
        {
            byte[] payload_bytes;
            ParsePacket(packet, out command, out payload_bytes);
            payload = StructInterop.ByteArrayToStruct<T>(payload_bytes);
        }

        #endregion
    }


    /// <summary>
    /// HLDC Framer Helper Class
    /// Takes care of receiving complete frames from an arbitrary byte stream.
    /// Uses an internal state machine to keep track of expected bytes and
    /// frame delimiters.
    /// 
    /// Usage:
    /// Call ProcessByte(b) until it returns true
    /// Complete frame data is then available in FrameBytes
    /// </summary>
    public class HLDCFramer
    {
        private enum State { waitingForSOF, waitingForHeader, readingFrame };
        private State state;
        private List<byte> buffer;
        private bool frameAvailable;

        /// <summary>
        /// A complete frame of bytes, or null if no frame available
        /// </summary>
        public byte[] FrameBytes { get { return (frameAvailable) ? buffer.ToArray() : null; } }

        public HLDCFramer()
        {
            state = State.waitingForSOF;
            buffer = new List<byte>();
            frameAvailable = false;
        }

        /// <summary>
        /// Process a single byte and add to the frame buffer once a frame has been found
        /// </summary>
        /// <returns>true if finished (frame found, or error)</returns>
        public bool ProcessByte(byte b)
        {
            frameAvailable = false;
            switch (state)
            {
                case State.waitingForSOF:
                    if (b == HLDCProtocol.HLDC_FRAME_DELIMITER)
                        state = State.waitingForHeader;
                    break;

                case State.waitingForHeader:
                    if (b != HLDCProtocol.HLDC_FRAME_DELIMITER)
                    {
                        buffer.Clear();
                        buffer.Add(b);
                        state = State.readingFrame;
                    }
                    break;

                case State.readingFrame:
                    if (b != HLDCProtocol.HLDC_FRAME_DELIMITER)
                    {
                        buffer.Add(b);

                        if (buffer.Count == HLDCProtocol.MAX_PACKET_LEN)
                        {
                            state = State.waitingForSOF;
                            throw new HLDCException("HLDC Framer Error - No EOF found");
                        }
                    }
                    else
                    {
                        if (buffer.Count < HLDCProtocol.MIN_PACKET_LEN)
                        {
                            state = State.waitingForSOF;
                            return true;
                            //throw new HLDCException("HLDC Framer Error - Not enough bytes for frame");
                        }

                        frameAvailable = true;
                        state = State.waitingForSOF;
                        return true; // Finished
                    }
                    break;

            }

            return false;
        }

        /// <summary>
        /// Reset the internal state machine and start looking for a new frame
        /// </summary>
        public void Reset()
        {
            state = State.waitingForSOF;
            frameAvailable = false;
        }
    }
}
