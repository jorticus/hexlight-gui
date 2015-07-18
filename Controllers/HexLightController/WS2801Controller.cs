using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexLight.Colour;

namespace HexLight.Control
{
    public abstract class WS2801ControllerSettings : ControllerSettings
    {
        private Int16 stringLength;
        private Int16 numStrings;

        public Int16 StringLength
        {
            get { return stringLength; }
            set { stringLength = value; PropChanged("StringLength"); }
        }

        public Int16 NumStrings
        {
            get { return numStrings; }
            set { numStrings = value; PropChanged("NumStrings"); }
        }
    }

    [ControllerName("WS2801 USB-HID")]
    //[ControllerSettingsType(typeof(HexControllerHIDSettings))]
    public abstract class WS2801Controller : HexBaseController
    {
        protected RGBColor color;

        public WS2801Controller() : base() { }
        public WS2801Controller(ControllerSettings settings) : base() { }

        #region Protocol Command Defs

        private const byte CMD_CONFIGURE_LAYOUT = 0x50;
        private const byte CMD_CLEAR = 0x51;
        private const byte CMD_SET_ALL = 0x52;
        private const byte CMD_SHIFT_IN = 0x53;
        private const byte CMD_SHIFT_ARRAY = 0x54;
        private const byte CMD_UPDATE = 0x55;

        #endregion

        #region Properties

        /// <summary>
        /// Set all LEDs in the matrix to the given colour
        /// </summary>
        public override RGBColor Color
        {
            get { return color; }
            set
            {
                color = value;
                SetAll(color);
                Update();
            }
        }

        public override float Brightness
        {
            get
            {
                return 1.0f;
            }
            set
            {
                // TODO
            }
        }

        #endregion

        #region Protocol Structs

        public struct LayoutConfigurationStruct
        {
            public Int16 StringLength;
            public Int16 NumStrings;
        }

        public struct RGBPixelStruct
        {
            public byte Red;
            public byte Green;
            public byte Blue;
        }

        #endregion

        /// <summary>
        /// Configure the physical layout of the WS2801 strings
        /// </summary>
        /// <param name="StringLength">The length of each string</param>
        /// <param name="NumStrings">The number of strings (Max: 10 strings)</param>
        public void ConfigureLayout(Int16 StringLength, Int16 NumStrings)
        {
            SendPacket<LayoutConfigurationStruct>(CMD_CONFIGURE_LAYOUT, new LayoutConfigurationStruct
            {
                StringLength = StringLength,
                NumStrings = NumStrings
            });
            ReadReply(CMD_CONFIGURE_LAYOUT);
        }

        /// <summary>
        /// Clear the internal LED buffer.
        /// NOTE: Use Update() to also clear the LEDs
        /// </summary>
        public void Clear()
        {
            SendPacket(CMD_CLEAR);
            ReadReply(CMD_CLEAR);
        }

        /// <summary>
        /// Update the LEDs with the contents of the internal LED buffer
        /// </summary>
        public void Update()
        {
            SendPacket(CMD_UPDATE);
            ReadReply(CMD_UPDATE);
        }

        /// <summary>
        /// Shift a pixel into the internal LED buffer.
        /// Pixels are shifted left to right, top to bottom
        /// </summary>
        /// <param name="colour">The colour to shift in</param>
        public void ShiftIn(RGBColor colour)
        {
            SendPacket<RGBPixelStruct>(CMD_SHIFT_IN, new RGBPixelStruct
            {
                Red = colour.Rb,
                Green = colour.Gb,
                Blue = colour.Bb
            });
            ReadReply(CMD_SHIFT_IN);
        }

        /// <summary>
        /// Set all entries of the internal LED buffer to the given colour
        /// </summary>
        /// <param name="colour">The colour to use</param>
        private void SetAll(RGBColor colour)
        {
            SendPacket<RGBPixelStruct>(CMD_SET_ALL, new RGBPixelStruct
            {
                Red = colour.Rb,
                Green = colour.Gb,
                Blue = colour.Bb
            });
            ReadReply(CMD_SET_ALL);
        }

    }
}
