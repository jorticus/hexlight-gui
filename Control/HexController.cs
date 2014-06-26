using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexLight.Util;
using HexLight.Util.ColorTypes;

namespace HexLight.Control
{
    public class HexController : RGBController, IDisposable
    {
        private const bool apply_cie = false;

        private RGBColor color;
        private float brightness;
        private string port;
        private int baud;
        private SerialPort serial;

        public enum Mode { HostControl, Trig, Cycle, Audio };

        #region Protocol Command Defs

        private const byte CMD_POWER_ON = 0x01;
        private const byte CMD_POWER_OFF = 0x02;
        private const byte CMD_SET_MODE = 0x03;
        private const byte CMD_GET_LAST_ERROR = 0x04;

        private const byte CMD_SET_PWM = 0x10;
        private const byte CMD_GET_PWM = 0x11;
        private const byte CMD_SET_XYZ = 0x12;
        private const byte CMD_GET_XYZ = 0x13;
        private const byte CMD_SET_XYY = 0x14;
        private const byte CMD_GET_XYY = 0x15;

        private const byte CMD_SET_XYZ_CAL = 0x20;
        private const byte CMD_GET_XYZ_CAL = 0x21;

        private const byte CMD_ACK = 0xFF;

        #endregion

        #region Protocol Structs

#pragma warning disable 0169 // Field XYZ is never used
#pragma warning disable 0649 // Field XYZ is never assigned to, and will always have its default value XX

        public struct PWMStruct
        {
            public Int16 CH1;  // Yes, signed ints. range is 0-32767, to match fractional Q15 datatype.
            public Int16 CH2;
            public Int16 CH3;
            public Int16 CH4;
        };

        private struct XYZStruct
        {
            public float X;
            public float Y;
            public float Z;
        };

        private struct XYYStruct
        {
            public float x;
            public float y;
            public float Y;
        }

        private struct XYZCalStruct
        {
            public byte ch;
            public XYYStruct point;
        }

#pragma warning restore 0169
#pragma warning restore 0649

        #endregion

        #region Properties

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

        #endregion

        #region Protocol Handlers

        /// <summary>
        /// Read a complete frame from the serial device.
        /// </summary>
        private byte[] ReceiveFrame()
        {
            HLDCFramer framer = new HLDCFramer();
            while (true)
            {
                serial.WriteTimeout = 1000;
                int b = serial.ReadByte();
                if ((b < 0) || (framer.ProcessByte((byte)b)))
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
        public void SendPacket<T>(byte command, T payload)
        {
            byte[] packet = HLDCProtocol.CreatePacket<T>(command, payload);
            serial.Write(packet, 0, packet.Length);
        }

        public void SendPacket(byte command, byte[] payload=null)
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
        public byte[] ReadReply(byte expected_command)
        {
            /*byte[] response = ReceiveFrame();
            byte command;
            byte[] payload;
            HLDCProtocol.ParsePacket(response, out command, out payload);

            if (command != expected_command)
                throw new Exception("HLDC Protocol Error - Invalid reply");

            return payload;*/
            return null;
        }

        /*public T ReadReply<T>(byte expected_command)
        {
            byte[] response = ReceiveFrame();
            byte command;
            T payload;
            HLDCProtocol.ParsePacket<T>(response, out command, out payload);

            if (command != expected_command)
                throw new Exception("HLDC Protocol Error - Invalid reply");

            return payload;
        }*/

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

        public void SetMode(Mode mode)
        {
            SendPacket(CMD_SET_MODE, new byte[] { (byte)mode });
            ReadReply(CMD_SET_MODE);
        }

        public void SetPWM(PWMStruct values)
        {
            SendPacket<PWMStruct>(CMD_SET_PWM, values);
            ReadReply(CMD_SET_PWM);
        }

        // Not valid, because the PWM value isn't available for reading
        /*public PWMStruct GetPWM()
        {
            SendPacket(CMD_GET_PWM);
            return ReadReply<PWMStruct>(CMD_GET_PWM);
        }*/

        public void SetXYZ(CIEXYZColour xyz)
        {
            SendPacket<XYZStruct>(CMD_SET_XYZ, new XYZStruct
            {
                X = (float)xyz.X, Y = (float)xyz.Y, Z = (float)xyz.Z
            });
            ReadReply(CMD_SET_XYZ);
        }

        public CIEXYZColour GetXYZ()
        {
            SendPacket(CMD_GET_XYZ);
            //var xyz = ReadReply<XYZStruct>(CMD_GET_XYZ);
            //return new CIEXYZColour(xyz.X, xyz.Y, xyz.Z);
            return new CIEXYZColour();
        }

        public void SetXYY(CIEXYYColor xyy)
        {
            SendPacket<XYYStruct>(CMD_SET_XYY, new XYYStruct
            {
                x = (float)xyy.x, y = (float)xyy.y, Y = (float)xyy.Y
            });
            ReadReply(CMD_SET_XYY);
        }

        public CIEXYYColor GetXYY()
        {
            SendPacket(CMD_GET_XYY);
            //var xyy = ReadReply<XYYStruct>(CMD_GET_XYY);
            //return new CIEXYYColor(xyy.x, xyy.y, xyy.Y);
            return new CIEXYYColor();
        }

        #endregion


        private float limit(float val) {
            if (val < 0.0f) return 0.0f;
            if (val > 1.0f) return 1.0f;
            return val;
        }

        private Int16 FloatToInt16(float x)
        {
            if (x < 0.0f) return 0;
            if (x > 1.0f) return Int16.MaxValue;
            return (Int16)(x * (float)Int16.MaxValue);
        }

        private void Update()
        {
            RGBColor rgb = this.color;//= CIE1931.CorrectRGB(this.color);
            SetPWM(new PWMStruct
            {
                CH1 = FloatToInt16(rgb.r),
                CH2 = FloatToInt16(rgb.g),
                CH3 = FloatToInt16(rgb.b),
                CH4 = 0
            });

        }


        public HexController(string port, int baud = 9600)
        {
            this.port = port;
            this.baud = baud;
            serial = new SerialPort(port, baud);
            serial.Open();
            this.Connected = true;
            this.brightness = 1.0f;
        }

        public void Dispose()
        {
            this.Connected = false;
            serial.Close();
        }
    }
}
