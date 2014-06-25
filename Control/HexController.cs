using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGB.Util;
using RGB.Util.ColorTypes;

namespace RGB.Control
{
    public class HexController : RGBController, IDisposable
    {
        private const bool apply_cie = false;

        private RGBColor color;
        private float brightness;
        private string port;
        private int baud;
        private SerialPort serial;

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

        private struct PWMStruct
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

            byte[] packet = HLDCParser.CreatePacket<PWMStruct>(0, new PWMStruct
            {
                CH1 = FloatToInt16(rgb.r),
                CH2 = FloatToInt16(rgb.g),
                CH3 = FloatToInt16(rgb.b),
                CH4 = 0
            });

            //var packet = HLDCParser.CreatePacket(0x7E, new byte[] { });
            /*foreach (var b in packet)
                serial.Write(new byte[] { b }, 0, 1);*/
            serial.Write(packet, 0, packet.Length);

            byte[] buf = new byte[100];
            serial.Read(buf, 0, buf.Length);
            if (buf[0] != 0)
                return;
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
