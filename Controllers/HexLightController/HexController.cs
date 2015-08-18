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
    public abstract class HexController : HexBaseController
    {
        private const bool apply_cie = false;

        protected RGBColor color;
        protected float brightness;

        public enum Mode { HostControl, Trig, Cycle, Audio };

        #region Protocol Command Defs

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

        private const byte CMD_ENABLE_USBAUDIO = 0x30;


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

        #region Protocol Implementation

        public void SetMode(Mode mode)
        {
            TransferPacket(CMD_SET_MODE, new byte[] { (byte)mode });
        }

        public void SetPWM(PWMStruct values)
        {
            TransferPacket<PWMStruct>(CMD_SET_PWM, values);
        }

        // Not valid, because the PWM value isn't available for reading
        /*public PWMStruct GetPWM()
        {
            SendPacket(CMD_GET_PWM);
            return ReadReply<PWMStruct>(CMD_GET_PWM);
        }*/

        public void SetXYZ(CIEXYZColour xyz)
        {
            TransferPacket<XYZStruct>(CMD_SET_XYZ, new XYZStruct
            {
                X = (float)xyz.X,
                Y = (float)xyz.Y,
                Z = (float)xyz.Z
            });
        }

        public CIEXYZColour GetXYZ()
        {
            var xyz = TransferPacket<XYZStruct>(CMD_GET_XYZ);
            return new CIEXYZColour(xyz.X, xyz.Y, xyz.Z);
        }

        public void SetXYY(CIEXYYColor xyy)
        {
            TransferPacket<XYYStruct>(CMD_SET_XYY, new XYYStruct
            {
                x = (float)xyy.x,
                y = (float)xyy.y,
                Y = (float)xyy.Y
            });
        }

        public CIEXYYColor GetXYY()
        {
            var xyy = TransferPacket<XYYStruct>(CMD_GET_XYY);
            return new CIEXYYColor(xyy.x, xyy.y, xyy.Y);
        }

        public void EnableUsbAudio(bool enabled)
        {
            TransferPacket(CMD_ENABLE_USBAUDIO, new byte[] { (byte)((enabled) ? 1 : 0) });
        }

        #endregion

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

        public HexController()
        {
            this.brightness = 1.0f;
        }
    }
}
