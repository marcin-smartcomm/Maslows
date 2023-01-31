using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace MaslowsMain
{
    public class WakeOnLAN
    {
        int _PORT;
        byte[] _MACADDRESS;
        IPAddress _IPADDRESS;
        ControlSystem _cs;

        byte[] _magicPacket;

        public WakeOnLAN(int port, string ipAddr, byte[] macAddrHex, ControlSystem cs)
        {
            _PORT = port;
            _IPADDRESS = IPAddress.Parse(ipAddr);
            _MACADDRESS = macAddrHex;
            _cs= cs;

            InitializeMagicPacket();
        }

        void InitializeMagicPacket()
        {
            _magicPacket = new byte[102];

            for (int i = 0; i < 6; i++)
                _magicPacket[i] = 0xFF;

            for (int i = 1; i < 17; i++)
                for (int j = 0; j < 6; j++)
                    _magicPacket[(6 * i) + j] = _MACADDRESS[j];
        }

        public byte[] GetMagicPacket() => _magicPacket;

        public void SendWakeOnLANMessage()
        {
            _cs.logger.WriteLine("Sending Wake On LAN to: " + _IPADDRESS + ", " + _MACADDRESS + " on port: " + _PORT);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint endPoint = new IPEndPoint(_IPADDRESS, _PORT);

            socket.SendTo(_magicPacket, _magicPacket.Length, SocketFlags.None, endPoint);
        }
    }
}
