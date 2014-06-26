using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using HexLight.Util;
using HexLight.Util.ColorTypes;

namespace HexLight.Control
{
    public class NetController : RGBController
    {
        private const bool apply_cie = true;

        private RGBColor color;
        private float brightness;
        private string host;
        private int port;
        private TcpClient socket;

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

        private void Update()
        {
            RGBColor value = color * brightness;
            RGBColor corrected = (apply_cie) ? CIE1931.CorrectRGB(value) : value;
            byte[] packet = { (byte)'X', corrected.Bb, corrected.Gb, corrected.Rb };
            socket.Client.Send(packet);
        }

        public NetController(string host, int port = 1234)
        {
            this.host = host;
            this.port = port;
            this.brightness = 1.0f;

            socket = new TcpClient();
            socket.Connect(host, port);
            socket.NoDelay = true;
        }

        public void Dispose()
        {
            socket.Close();
        }
    }
}
