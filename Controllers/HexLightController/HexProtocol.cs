using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexLight.Util;

namespace HexLight.Control
{
    class HexProtocol
    {
        #region Definitions

        public const byte CMD_ERROR = 0xFF;

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

        public static UInt16 CalculateCrc(byte[] buffer)
        {
            return 0; // Yeaaaa
        }

        #endregion

        #region Send Packet

        public static byte[] CreatePacket(byte command, byte[] payload = null)
        {
            List<byte> packetBuilder = new List<byte>();

            if (payload != null && payload.Length > MAX_PAYLOAD_LEN)
                throw new Exception("Payload too large");

            // Build the packet header
            HeaderStruct header = new HeaderStruct()
            {
                command = command,
                length = (byte)((payload != null) ? payload.Length : 0)
            };
            packetBuilder.AddRange(StructInterop.StructToByteArray<HeaderStruct>(header));

            // Build the packet payload
            UInt16 crc = 0;
            if (payload != null)
            {
                packetBuilder.AddRange(payload);
                crc = CalculateCrc(payload);
            }

            // BUild the packet footer
            FooterStruct footer = new FooterStruct()
            {
                crc = crc,
            };
            packetBuilder.AddRange(StructInterop.StructToByteArray<FooterStruct>(footer));

            if (packetBuilder.Count > MAX_PACKET_LEN)
                throw new Exception("Packet too large");

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
            var headerBytes = new ArraySegment<byte>(packet, 0, HEADER_SIZE).ToArray();
            HeaderStruct header = StructInterop.ByteArrayToStruct<HeaderStruct>(headerBytes);

            // Payload
            if (header.length > 0)
                payload = new ArraySegment<byte>(packet, HEADER_SIZE, header.length).ToArray();
            else
                payload = null;

            var footerBytes = new ArraySegment<byte>(packet, HEADER_SIZE + header.length, FOOTER_SIZE).ToArray();
            FooterStruct footer = StructInterop.ByteArrayToStruct<FooterStruct>(footerBytes);

            // Detect protocol errors
            if (header.command == CMD_ERROR)
            {
                if (payload != null && payload.Length > 0)
                    throw HLDCProtocolException.FromCode(payload[0]);
                else
                    throw new HLDCException("Protocol malformed error message");
            }

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
}
