using Crestron.SimplSharpPro.EthernetCommunication;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public class LGTV
    {
        ControlSystem _cs;
        AsyncTCPClient _comms;
        string _IPADDRESS;
        int _PORT;
        byte[] _MACADDRESS;
        WakeOnLAN _wakeOnLAN;

        public string currentSource;
        public int volLevel;

        public string TVName;

        public event Action<bool> TVConnectedEvent;
        public event Action<int> VolChangeEvent;

        public LGTV(ControlSystem cs, string name, string ipAddr, int port, byte[] macAddr)
        {
            this._cs = cs;

            this.TVName = name;
            _IPADDRESS = ipAddr;
            _PORT = port;
            _MACADDRESS = macAddr;

            _wakeOnLAN = new WakeOnLAN(6, ipAddr, _MACADDRESS);

            _comms = new AsyncTCPClient(cs, ipAddr, port, 4000);
            _comms.MessageReceived += OnMessageReceived;
            _comms.ConnectedEvent += OnDeviceConnected;
        }
        public void ConnectRequest(int tpID)
        {
            if (currentSource != "Off")
                _comms.ConnectRequest(tpID);
        }
        public void Disconnect(int tpID) => _comms.Disconnect(tpID);
        public bool GetConnectionStatus() => _comms.GetConnectionStatus();


        void OnMessageReceived(object source, MessageReceivedEventArgs e)
        {
            string textToProcess = Encoding.ASCII.GetString(e.message);
            _cs.logger.WriteLine("Received From " + TVName + ": " + textToProcess);
            evaluateResponse(textToProcess);
        }
        void OnDeviceConnected(bool connStatus)
        {
            if (this.TVConnectedEvent != null)
                this.TVConnectedEvent(connStatus);
        }
        void OnVolumeChange(int volLevel)
        {
            if (this.VolChangeEvent != null)
                this.VolChangeEvent(volLevel);
        }

        public int GetVolumeLevel() => volLevel;
        public void PowerOn() => _wakeOnLAN.SendWakeOnLANMessage();
        public void PowerOff() => _comms.SendMessage("ka 00 00\r");
        public void VolUp()
        {
            if(volLevel >= 95)
                _comms.SendMessage("kf 00 64\r");
            else
            {
                int newVolume = volLevel + 5;
                string newVolumeHex = newVolume.ToString("X2");
                _comms.SendMessage("kf 00 " + newVolumeHex + "\r");
            }
        }
        public void VolDown()
        {
            if (volLevel <= 5)
                _comms.SendMessage("kf 00 00\r");
            else
            {
                int newVolume = volLevel - 5;
                string newVolumeHex = newVolume.ToString("X2");
                _comms.SendMessage("kf 00 " + newVolumeHex + "\r");
            }
        }

        int HDMISelect(int hdmiInput)
        {
            if(_comms.GetConnectionStatus())
                _comms.SendMessage("xb 00 9" + (hdmiInput-1) + "\r");
            return 1;
        }
        static int Delay(int toReturnAfterDelay, int milisecondDelay)
        {
            Thread.Sleep(milisecondDelay);

            return toReturnAfterDelay;
        }

        static void Delay(int milisecondDelay)
        {
            Thread.Sleep(milisecondDelay);
        }

        public void SourceSelectedChanged(string source, int tpID)
        {
            if (source.Equals("Off"))
            {
                PowerOff();
                currentSource = source;
            }
            else
                PowerOn();

            if (currentSource.Equals("Off") && !source.Equals("Off"))
            {
                currentSource = source;

                Task.Run(() =>
                {
                    Delay(7000);
                    _comms.ConnectRequest(tpID);
                });
            }

            if(source.Equals("IPTV"))
            {
                HDMISelect(1);
                Task.Run(() =>
                {
                   HDMISelect(Delay(1, 8000));
                });
            }
            else if (source.Equals("Mersive"))
            {
                HDMISelect(2);
                Task.Run(()=>
                {
                    HDMISelect(Delay(2, 8000));
                });
            }
            else if (source.Equals("Laptop"))
            {
                HDMISelect(3);
                Task.Run(() =>
                {
                    HDMISelect(Delay(3, 8000));
                });
            }
        }

        void evaluateResponse(string textToProcess)
        {
            if (textToProcess.Contains("f ") && textToProcess.Contains("OK"))
            {
                string volStr = textToProcess.Remove(0, 7);
                volStr = volStr.Remove(2, 1);
                _cs.logger.WriteLine(volStr);

                volLevel = int.Parse(volStr, System.Globalization.NumberStyles.HexNumber);

                OnVolumeChange(volLevel);
            }
        }
    }
}
