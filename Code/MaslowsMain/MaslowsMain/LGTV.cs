using Crestron.SimplSharpPro.EthernetCommunication;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public class LGTV
    {
        ControlSystem cs;
        AsyncTCPClient comms;
        string IPADDRESS;
        int PORT;

        public int volLevel;

        public string TVName;

        public event Action<bool> TVConnectedEvent;
        public event Action<int> VolChangeEvent;

        public LGTV(ControlSystem cs, string name, string ipAddr, int port)
        {
            this.cs = cs;

            this.TVName = name;
            IPADDRESS = ipAddr;
            PORT = port;

            comms = new AsyncTCPClient(cs, ipAddr, port, 4000);
            comms.MessageReceived += OnMessageReceived;
            comms.ConnectedEvent += OnDeviceConnected;
        }

        public void Connect() => comms.Connect();
        public void ConnectRequest(int tpID) => comms.ConnectRequest(tpID);
        public void Disconnect(int tpID) => comms.Disconnect(tpID);
        public bool GetConnectionStatus() => comms.GetConnectionStatus();

        private void OnMessageReceived(object source, MessageReceivedEventArgs e)
        {
            string textToProcess = Encoding.ASCII.GetString(e.message);
            cs.logger.WriteLine("Received From " + TVName + ": " + textToProcess);
            evaluateResponse(textToProcess);
        }

        static void Delay(int milisecondsDelay)
        {
            Thread.Sleep(milisecondsDelay);
            return;
        }

        void evaluateResponse(string textToProcess)
        {
            if(textToProcess.Contains("f ") && textToProcess.Contains("OK"))
            {
                string volStr = textToProcess.Remove(0, 7);
                volStr = volStr.Remove(2, 1);
                cs.logger.WriteLine(volStr);

                volLevel = int.Parse(volStr, System.Globalization.NumberStyles.HexNumber);

                OnVolumeChange(volLevel);
            }
        }

        void OnDeviceConnected(bool connStatus)
        {
            if (this.TVConnectedEvent != null)
            {
                this.TVConnectedEvent(connStatus);
            }
        }

        void OnVolumeChange(int volLevel)
        {
            if (this.VolChangeEvent != null)
            {
                this.VolChangeEvent(volLevel);
            }
        }

        public int GetVolumeLevel() => volLevel;

        public void PowerOn()
        {
            comms.SendMessage("ka 00 01\r");
        }

        public void PowerOff()
        {
            comms.SendMessage("ka 00 00\r");
        }

        public void VolUp()
        {
            if(volLevel >= 95)
            {
                comms.SendMessage("kf 00 64\r");
            }
            else
            {
                int newVolume = volLevel + 5;
                string newVolumeHex = newVolume.ToString("X2");
                comms.SendMessage("kf 00 " + newVolumeHex + "\r");
            }
        }
        public void VolDown()
        {
            if (volLevel <= 5)
            {
                comms.SendMessage("kf 00 00\r");
            }
            else
            {
                int newVolume = volLevel - 5;
                string newVolumeHex = newVolume.ToString("X2");
                comms.SendMessage("kf 00 " + newVolumeHex + "\r");
            }
        }

        public int HDMISelect(int hdmiInput)
        {
            comms.SendMessage("xb 00 9" + (hdmiInput-1) + "\r");
            return 1;
        }

        static int Delay(int toReturnAfterDelay, int milisecondDelay)
        {
            Thread.Sleep(milisecondDelay);

            return toReturnAfterDelay;
        }

        public void SourceSelectedChanged(string source)
        {
            if (source.Equals("Off"))
                PowerOff();
            else
                PowerOn();

            if(source.Equals("IPTV"))
            {
                HDMISelect(1);
                Task.Run(() =>
                {
                   HDMISelect(Delay(1, 3000));
                });
            }
            else if (source.Equals("Mersive"))
            {
                HDMISelect(2);
                Task.Run(()=>
                {
                    HDMISelect(Delay(2, 3000));
                });
            }
            else if (source.Equals("Laptop"))
            {
                HDMISelect(3);
                Task.Run(() =>
                {
                    HDMISelect(Delay(3, 3000));
                });
            }
        }
    }
}
