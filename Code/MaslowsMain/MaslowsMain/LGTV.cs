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
        byte[] _MACADDRESS;
        WakeOnLAN _wakeOnLAN;

        public string currentSource;
        public int volLevel, volLevelAtPowerOff;

        public string TVName;

        public event Action<bool> TVConnectedEvent;
        public event Action<int> VolChangeEvent;

        public LGTV(ControlSystem cs, string name, string ipAddr, int port, byte[] macAddr)
        {
            this._cs = cs;

            this.TVName = name;
            _MACADDRESS = macAddr;

            _wakeOnLAN = new WakeOnLAN(6, ipAddr, _MACADDRESS, _cs);

            _comms = new AsyncTCPClient(cs, ipAddr, port, 4000);
            _comms.MessageReceived += OnMessageReceived;
            _comms.ConnectedEvent += OnDeviceConnected;
        }
        public void ConnectRequest(int tpID)
        {
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
        public void PowerOn()
        {
            Task.Run(() =>
            {
                //Backlight Brightness 100%
                _comms.SendMessage("mg 01 64\r");
                Thread.Sleep(500);

                //Screen Mute Off
                _comms.SendMessage("kd 01 00\r");
                Thread.Sleep(500);

                //Return to volume at power off
                string newVolumeHex = volLevelAtPowerOff.ToString("X2");
                _comms.SendMessage("kf 01 " + newVolumeHex + "\r");
                Thread.Sleep(500);

                //TV should be always On
                _comms.SendMessage("ka 01 01\r");
            });
        }
        public void PowerOff()
        {
            volLevelAtPowerOff = volLevel;

            Task.Run(() =>
            {
                //Backlight Brightness 0%
                _comms.SendMessage("mg 01 00\r");
                Thread.Sleep(500);

                //Screen Mute On
                _comms.SendMessage("kd 01 01\r");
                Thread.Sleep(500);

                //Kill Volume
                _comms.SendMessage("kf 01 00\r");
            });
        }
        public void VolUp()
        {
            if(volLevel >= 95)
                _comms.SendMessage("kf 01 64\r");
            else
            {
                int newVolume = volLevel + 5;
                string newVolumeHex = newVolume.ToString("X2");
                _comms.SendMessage("kf 01 " + newVolumeHex + "\r");
            }
        }
        public void VolDown()
        {
            if (volLevel <= 5)
                _comms.SendMessage("kf 01 00\r");
            else
            {
                int newVolume = volLevel - 5;
                string newVolumeHex = newVolume.ToString("X2");
                _comms.SendMessage("kf 01 " + newVolumeHex + "\r");
            }
        }

        int HDMISelect(int hdmiInput)
        {
            if(_comms.GetConnectionStatus())
                _comms.SendMessage("xb 01 9" + (hdmiInput-1) + "\r");
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
            bool wasOff = false;

            if (source.Equals("Off"))
            {
                PowerOff();
                currentSource = source;
            }

            if (currentSource.Equals("Off") && !source.Equals("Off"))
            {
                wasOff = true;
                currentSource = source;
                PowerOn();
            }

            if(source.Equals("IPTV"))
            {
                if(!wasOff)
                    HDMISelect(1);
                else
                {
                    Task.Run(() =>
                    {
                        HDMISelect(Delay(1, 2000));
                    });
                }
            }
            else if (source.Equals("Mersive"))
            {
                if (!wasOff)
                    HDMISelect(2);
                else
                {
                    Task.Run(() =>
                    {
                        HDMISelect(Delay(2, 2000));
                    });
                }
            }
            else if (source.Equals("Laptop"))
            {
                if (!wasOff)
                    HDMISelect(2);
                else
                {
                    Task.Run(() =>
                    {
                        HDMISelect(Delay(2, 2000));
                    });
                }
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
