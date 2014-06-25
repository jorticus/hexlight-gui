using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RGB.Control
{
    /// <summary>
    /// High-Level Data Link Control Parser
    /// http://en.wikipedia.org/wiki/High-Level_Data_Link_Control
    /// </summary>
    public class HLDCParser
    {
        private const byte HLDC_FRAME_DELIMITER = 0x7E;
        private const byte HLDC_ESCAPE = 0x7D;
        private const byte HLDC_ESCAPE_MASK = 0x20;

        private static readonly int MAX_PACKET_LEN = 64;
        private static readonly int HEADER_SIZE = Marshal.SizeOf(typeof(HeaderStruct));
        private static readonly int FOOTER_SIZE = Marshal.SizeOf(typeof(HeaderStruct));
        private static readonly int MIN_PACKET_LEN = HEADER_SIZE - FOOTER_SIZE;
        private static readonly int MAX_PAYLOAD_LEN = MAX_PACKET_LEN - HEADER_SIZE - FOOTER_SIZE;


        [StructLayout(LayoutKind.Sequential, Pack=1)]
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

        private static byte[] StructToByteArray<T>(T str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

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

        public static byte[] CreatePacket<T>(byte command, T struc)
        {
            byte[] payload = StructToByteArray<T>(struc);
            return CreatePacket(command, payload);
        }

        public static UInt16 CalculateCrc(byte[] buffer)
        {
            return 0;
        }

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
            packetBuilder.AddRange(EscapeBytes(StructToByteArray<HeaderStruct>(header)));

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
            packetBuilder.AddRange(EscapeBytes(StructToByteArray<FooterStruct>(footer)));

            packetBuilder.Add(HLDC_FRAME_DELIMITER);

            if (packetBuilder.Count > MAX_PACKET_LEN)
                throw new Exception("HLDC packet too large");

            return packetBuilder.ToArray();
        }

        public static void ParsePacket(byte[] packet, out byte command, out byte[] payload)
        {
            payload = new byte[1];
            command = 0;
        }
    }
}
