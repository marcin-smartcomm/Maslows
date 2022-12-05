using Crestron.SimplSharpPro.EthernetCommunication;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public class IPTV
    {
        public ControlSystem cs;
        AsyncTCPClient comms;

        public string name;

        public event Action<bool> IPTVConnectedEvent;

        public IPTV(string ipAddr, int port, string name, ControlSystem contsys)
        {
            cs = contsys;

            this.name = name;

            comms = new AsyncTCPClient(contsys, ipAddr, port, 4000);
            comms.MessageReceived += OnMessageReceived;
            comms.ConnectedEvent += OnDeviceConnected;
        }

        private void OnMessageReceived(object source, MessageReceivedEventArgs e)
        {
            string textToProcess = Encoding.ASCII.GetString(e.message);
            cs.logger.WriteLine("Received From " + name + ": " + textToProcess);
        }

        void OnDeviceConnected(bool connStatus)
        {
            if (this.IPTVConnectedEvent != null)
            {
                this.IPTVConnectedEvent(connStatus);
            }
        }

        public void Connect() => comms.Connect();
        public void ConnectRequest(int tpID) => comms.ConnectRequest(tpID);
        public void Disconnect(int tpID) => comms.Disconnect(tpID);
        public bool GetConnectionStatus() => comms.GetConnectionStatus();

        public void PushButton(int btnPressed)
        {
            switch (btnPressed)
            {
                //Directional Pad
                case 0:
                    comms.SendMessage("Up\x0a");
                    break;
                case 1:
                    comms.SendMessage("Left\x0a");
                    break;
                case 2:
                    comms.SendMessage("Select\x0a");
                    break;
                case 3:
                    comms.SendMessage("Right\x0a");
                    break;
                case 4:
                    comms.SendMessage("Down\x0a");
                    break;

                //Ch + -
                case 5:
                    comms.SendMessage("Ch+\x0a");
                    break;
                case 6:
                    comms.SendMessage("Ch-\x0a");
                    break;

                //Function Btns
                case 7:
                    comms.SendMessage("Menu\x0a");
                    break;
                case 8:
                    comms.SendMessage("Guide\x0a");
                    break;

                //Numpad
                case 9:
                    comms.SendMessage("1\x0a");
                    break;
                case 10:
                    comms.SendMessage("2\x0a");
                    break;
                case 11:
                    comms.SendMessage("3\x0a");
                    break;
                case 12:
                    comms.SendMessage("4\x0a");
                    break;
                case 13:
                    comms.SendMessage("5\x0a");
                    break;
                case 14:
                    comms.SendMessage("6\x0a");
                    break;
                case 15:
                    comms.SendMessage("7\x0a");
                    break;
                case 16:
                    comms.SendMessage("8\x0a");
                    break;
                case 17:
                    comms.SendMessage("9\x0a");
                    break;
                case 18:
                    comms.SendMessage("0\x0a");
                    break;

                //Color Btns
                case 19:
                    comms.SendMessage("Red\x0a");
                    break;
                case 20:
                    comms.SendMessage("Green\x0a");
                    break;
                case 21:
                    comms.SendMessage("Yellow\x0a");
                    break;
                case 22:
                    comms.SendMessage("Blue\x0a");
                    break;
            }
        }
    }
}
