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
        //AsyncTCPClient _comms;
        byte[] _MACADDRESS;
        WakeOnLAN _wakeOnLAN;

        public string currentSource;
        public int volLevel, volLevelAtPowerOff;

        public string TVName;

        public event Action<bool> TVConnectedEvent;
        public event Action<int> VolChangeEvent;
        public event Action<bool> MuteStateChangedEvent;
        public event Action<bool> TVSelectedEvent;

        public LGTV(ControlSystem cs, string name, string ipAddr, int port, byte[] macAddr)
        {
            this._cs = cs;

            this.TVName = name;
            _MACADDRESS = macAddr;

            _wakeOnLAN = new WakeOnLAN(6, ipAddr, _MACADDRESS, _cs);

            //_comms = new AsyncTCPClient(cs, ipAddr, port, 4000);
            //_comms.MessageReceived += OnMessageReceived;
            //_comms.ConnectedEvent += OnDeviceConnected;
        }
        public void ConnectRequest(int tpID)
        {
            //_comms.ConnectRequest(tpID);
        }
        //public void Disconnect(int tpID) => _comms.Disconnect(tpID);
        //public bool GetConnectionStatus() => _comms.GetConnectionStatus();


        void OnMessageReceived(object source, MessageReceivedEventArgs e)
        {
            string textToProcess = Encoding.ASCII.GetString(e.message);
            _cs.logger.WriteLine("Received From " + TVName + ": " + textToProcess);
            evaluateResponse(textToProcess);
        }
        void OnDeviceConnected(bool connStatus)
        {
            if (TVConnectedEvent != null)
                TVConnectedEvent(connStatus);
        }
        void OnVolumeChange(int volLevel)
        {
            if (VolChangeEvent != null)
                VolChangeEvent(volLevel);
        }

        void OnMuteStateChagne(bool newState)
        {
            if (MuteStateChangedEvent != null)
                MuteStateChangedEvent(newState);
        }

        void OnTVSelectedEvent(bool state)
        {
            if (TVSelectedEvent != null)
                TVSelectedEvent(state);
        }

        public int GetVolumeLevel() => volLevel;
        public void PowerOn()
        {
            _cs.SendMessage(TVName + ":PowerOn");
        }
        public void PowerOff()
        {
            _cs.SendMessage(TVName + ":PowerOff");
        }
        public void VolUp()
        {
            _cs.SendMessage(TVName + ":VolUp");
        }
        public void VolDown()
        {
            _cs.SendMessage(TVName + ":VolDown");
        }

        public void Mute()
        {
            _cs.SendMessage(TVName + ":Mute");
        }

        int HDMISelect(int hdmiInput)
        {
            _cs.SendMessage(TVName + ":HDMI" + hdmiInput);
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

        public void SourceSelectedChanged(string source)
        {
            try
            {
                if (!source.Equals("Off"))
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        PowerOn();
                    });
                }
                _cs.logger.WriteLine(TVName + ": Changing source to " + source);

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

                if (source.Equals("IPTV") || source.Equals("TV"))
                {
                    try
                    {
                        _cs.SendMessage(TVName + ":HDMI1");
                    }catch(Exception e)
                    {
                        _cs.logger.WriteLine("Exception in LGTV.SourceSelectedChanged().SendMessage: " + e);
                    }
                    OnTVSelectedEvent(true);
                }
                else if (source.Equals("Laptop") || source.Equals("Wireless") || source.Equals("COLLABORATE"))
                {
                    _cs.SendMessage(TVName + ":Laptop");
                }
                else if (source.Equals("Sky"))
                {
                    if (!wasOff)
                        HDMISelect(1);
                    else
                    {
                        Task.Run(() =>
                        {
                            HDMISelect(Delay(1, 2000));
                        });
                    }
                }
                else if (source.Equals("MERSIVE"))
                {
                    try
                    {
                        _cs.SendMessage(TVName + ":HDMI3");
                    }
                    catch (Exception e)
                    {
                        _cs.logger.WriteLine("Exception in LGTV.SourceSelectedChanged().SendMessage: " + e);
                    }
                }
            }catch(Exception ex)
            {
                _cs.logger.WriteLine("Exception in LGTV.SourceSelectedChanged(): " + ex);
            }
        }

        public void VolumeChanged(int volLevel)
        {
            OnVolumeChange(volLevel);
        }

        public void MuteStateChanged(bool newState)
        {
            OnMuteStateChagne(newState);
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
