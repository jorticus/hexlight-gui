using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using RGB.Util;
using RGB.Util.ColorTypes;

namespace RGB.Control
{
    public class NetController : RGBController
    {
        private const bool apply_cie = true;

        private RGBColor color;
        private string host;
        private int port;
        private TcpClient socket;

        public override RGBColor Color
        {
            get { return color; }
            set
            {
                color = value;

                RGBColor corrected = (apply_cie) ? CIE1931.CorrectRGB(value) : value;
                byte[] packet = { (byte)'X', corrected.Bb, corrected.Gb, corrected.Rb };
                socket.Client.Send(packet);
            }
        }

        public NetController(string host, int port = 1234)
        {
            this.host = host;
            this.port = port;

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
