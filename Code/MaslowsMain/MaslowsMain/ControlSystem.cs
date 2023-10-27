using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;            // For Threadingb
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.DeviceSupport;

namespace MaslowsMain
{
    public class ControlSystem : CrestronControlSystem
    {
        MasterIpad masteriPadController;
        IPadController iPadController;
        CrestronGo masterIPad, diningIPad;
        
        public List<Room> rooms;
        public ConsoleLogger logger;
        public Touchpannel[] tp;
        public Touchpannel tpDecider;
        public IPTV[] iptvs;
        public LGTV[] TVs;
        IROutputPort _sky1_IRPort;
        public ThreeSeriesTcpIpEthernetIntersystemCommunications _SimplWindowsComms;

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        private void FireAlarmRelay_VersiportChange(Versiport port, VersiportEventArgs args)
        {
            logger.WriteLine("Port" + port.DeviceName + "state changed to: " + args.Event + "Digital In State: " + port.DigitalIn);

            FireAlarmState(!port.DigitalIn);

            if (!port.DigitalIn)
                logger.WriteLine("FireAlarm recorded at: " + DateTime.Now);
        }
        public void FireAlarmState(bool state)
        {
            try
            {
                //for (int i = 0; i < tp.Length; i++)
                //    tp[i].OnFireAlarmStateChange(state);

                //logger.WriteLine("Informed TPs of fire alarm state");

                if(state)
                {
                    logger.WriteLine("Closing Relay...");
                    this.RelayPorts[1].Close();
                    logger.WriteLine("Relay1 - Closed");
                }
                else
                {
                    logger.WriteLine("Openning Relay...");
                    this.RelayPorts[1].Open();
                    logger.WriteLine("Relay1 - Open");
                }
            }
            catch (Exception ex)
            {
                logger.WriteLine("Exception While Informing: " + ex);
            }
        }

        public void CloseRelays()
        {
            Task.Run(() =>
            {
                for(uint i = 1; i < 2;i++)
                {
                    this.RelayPorts[i].Close();
                    Thread.Sleep(1000);
                }
            });
        }

        public void OpenRelays()
        {
            Task.Run(() =>
            {
                for (uint i = 1; i < 2; i++)
                {
                    this.RelayPorts[i].Open();
                    Thread.Sleep(1000);
                }
            });
        }

        void InitializeEquipment()
        {
            try
            {
                iptvs = new IPTV[11];
                TVs = new LGTV[11];

                iptvs[0] = new IPTV("10.10.10.58", 7070, "iptv1", this);
                iptvs[1] = new IPTV("10.10.10.57", 7070, "iptv2", this);
                iptvs[2] = new IPTV("10.10.10.55", 7070, "iptv3", this);
                iptvs[3] = new IPTV("10.10.10.51", 7070, "iptv4", this);
                iptvs[4] = new IPTV("10.10.10.56", 7070, "iptv5", this);
                iptvs[5] = new IPTV("10.10.10.50", 7070, "iptv6", this);
                iptvs[6] = new IPTV("10.10.10.60", 7070, "iptv7", this);
                iptvs[7] = new IPTV("10.10.10.59", 7070, "iptv8", this);
                iptvs[8] = new IPTV("10.10.10.54", 7070, "iptv9", this);
                iptvs[9] = new IPTV("10.10.10.53", 7070, "iptv10", this);
                iptvs[10] = new IPTV("10.10.10.52", 7070, "iptv11", this);

                TVs[0] = new LGTV(this, "TV1", "172.16.30.80", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[1] = new LGTV(this, "TV2", "172.16.30.81", 9761, new byte[] { 0x74, 0xE6, 0xB8, 0x4F, 0x60, 0x98 });
                TVs[2] = new LGTV(this, "TV3", "172.16.30.82", 9761, new byte[] { 0x74, 0xE6, 0xB8, 0x4F, 0x63, 0x59 });
                TVs[3] = new LGTV(this, "TV4", "172.16.30.83", 9761, new byte[] { 0x74, 0xE6, 0xBB, 0x4F, 0x60, 0xA6 });
                TVs[4] = new LGTV(this, "TV5", "172.16.30.84", 9761, new byte[] { 0x74, 0xE6, 0xB8, 0x4F, 0x60, 0x96 });
                TVs[5] = new LGTV(this, "TV6", "172.16.30.85", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[6] = new LGTV(this, "TV7", "172.16.30.86", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[7] = new LGTV(this, "TV8", "172.16.30.87", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[8] = new LGTV(this, "TV9", "172.16.30.88", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[9] = new LGTV(this, "TV10", "172.16.30.89", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[10] = new LGTV(this, "TV11", "172.16.30.90", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
            }
            catch(Exception ex)
            {
                logger.WriteLine("Problem in Initialize Equipment: " + ex.Message);
            }
        }
        void InitializeRooms()
        {
            rooms = new List<Room>();

            try
            {
                rooms.Add(new Room(1, iptvs[0], TVs[0], this));    //Room 5.11 - Meeting Room 501
                rooms.Add(new Room(2, iptvs[1], TVs[1], this));     //Room 4.17 - Meeting Room 401
                rooms.Add(new Room(3, iptvs[2], TVs[2], this));     //Room 4.16 - Lounge 402
                rooms.Add(new Room(4, iptvs[3], TVs[3], this));     //Room 3.18 - Meeting Room 301
                rooms.Add(new Room(5, iptvs[4], TVs[4], this));     //Room 3.17 - Lounge 302
                rooms.Add(new Room(6, iptvs[5], TVs[5], this));     //Room 2.17 - Meeting Room 204
                rooms.Add(new Room(7, iptvs[6], TVs[6], this));     //Room 2.16 - Meeting Room 203
                rooms.Add(new Room(8, iptvs[7], TVs[7], this));     //Room 2.15 - Meeting Room 202
                rooms.Add(new Room(9, iptvs[8], TVs[8], this));     //Room 2.14 - Meeting Room 201
                rooms.Add(new Room(10, iptvs[9], TVs[9], this));    //Room L1 Event Space
                rooms.Add(new Room(11, iptvs[10], TVs[10], this));   //Room L1 Private Dining
            }
            catch (Exception ex)
            {
                logger.WriteLine("Problem in InitializeRooms: " + ex.Message);
            }
        }
        void InitializeTPs()
        {
            const ushort TOUCHPANNEL_START_PORT = 50000;

            masterIPad = new CrestronGo(0x10, this);
            masteriPadController = new MasterIpad(masterIPad, rooms, this, 1);

            diningIPad = new CrestronGo(0x11, this);
            iPadController = new IPadController(diningIPad, rooms[10], this);

            tpDecider = new Touchpannel(50000, rooms[0], this);

            tp = new Touchpannel[9];
            tp[0] = new Touchpannel(TOUCHPANNEL_START_PORT + 1, rooms[0], this);
            tp[1] = new Touchpannel(TOUCHPANNEL_START_PORT + 2, rooms[1], this);
            tp[2] = new Touchpannel(TOUCHPANNEL_START_PORT + 3, rooms[3], this);
            tp[3] = new Touchpannel(TOUCHPANNEL_START_PORT + 4, rooms[5], this);
            tp[4] = new Touchpannel(TOUCHPANNEL_START_PORT + 5, rooms[6], this);
            tp[5] = new Touchpannel(TOUCHPANNEL_START_PORT + 6, rooms[8], this);
            tp[6] = new Touchpannel(TOUCHPANNEL_START_PORT + 7, rooms[9], this);
            tp[7] = new Touchpannel(TOUCHPANNEL_START_PORT + 8, rooms[10], this);
            tp[8] = new Touchpannel(TOUCHPANNEL_START_PORT + 9, rooms[7], this);
        }

        public override void InitializeSystem()
        {
            try
            {
                if (this.SupportsEthernet)
                {
                    logger = new ConsoleLogger(55555, this);

                    InitializeEquipment();
                    InitializeRooms();
                    InitializeTPs();
                }

                for (int i = 0; i < tp.Length; i++)
                {
                    tp[i].Start();
                }
                tpDecider.Start();
                logger.Start();

                for(uint i = 1; i < 9; i++)
                {
                    if (this.RelayPorts[i].Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    {
                        logger.WriteLine("Error Registering Relay" + i +": {0}", this.RelayPorts[i].DeviceRegistrationFailureReason);
                    }
                }
                if (this.VersiPorts[1].Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    logger.WriteLine("Error Registering Versiport1: {0}", this.VersiPorts[1].DeviceRegistrationFailureReason);
                }
                else
                {
                    if (this.VersiPorts[1].SupportsDigitalInput)
                    {
                        logger.WriteLine("Configuring versiport as Digital In");
                        this.VersiPorts[1].SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
                    }

                    this.VersiPorts[1].VersiportChange += FireAlarmRelay_VersiportChange;
                }
                logger.WriteLine("Registering IR Devices...");
                if (ControllerIROutputSlot.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    logger.WriteLine("Problem Registering IR Devices: " + ControllerIROutputSlot.DeviceRegistrationFailureReason);
                else
                {
                    string IRPath = string.Format
                        (
                        "{0}/nvram/SkyHD.ir", 
                        Crestron.SimplSharp.CrestronIO.Directory.GetDirectoryRoot(Crestron.SimplSharp.CrestronIO.Directory.GetApplicationDirectory())
                        );
                    logger.WriteLine("getting IR file from: " + IRPath);

                    _sky1_IRPort = IROutputPorts[1];
                    logger.WriteLine("Sky1 IR Ports Registered successfully");
                    _sky1_IRPort.LoadIRDriver(IRPath);
                    logger.WriteLine("Sky1 IR Driver Loaded successfully");

                    foreach (string s in _sky1_IRPort.AvailableIRCmds())
                        logger.WriteLine("Sky IR: {0}", s);
                }

                try
                {
                    _SimplWindowsComms = new ThreeSeriesTcpIpEthernetIntersystemCommunications(0xB0, "127.0.0.2", this);
                    if (_SimplWindowsComms.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        logger.WriteLine("Failed To Register Comms with Simpl Windows");
                    else
                    {
                        logger.WriteLine("_Simpl windows comms registered");
                        _SimplWindowsComms.SigChange += _SimplWindowsComms_SigChange;
                    }
                }
                catch (Exception ex)
                {
                    logger.WriteLine("Problem in ControlSystem InitializeEquipment: " + ex);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void _SimplWindowsComms_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.String:
                    if(args.Sig.Number == 1)
                    {
                        if(_SimplWindowsComms.StringOutput[1].StringValue.Equals("WakeSystem"))
                        {
                            WakeSystem();
                        }
                    }
                    else
                    {
                        logger.WriteLine("Vol level coming on join: " + args.Sig.Number);
                        TVs[args.Sig.Number - 10].VolumeChanged(int.Parse(_SimplWindowsComms.StringOutput[args.Sig.Number].StringValue));
                    }
                    break;

                case eSigType.Bool:
                    logger.WriteLine("Bool signal change on join: " + args.Sig.Number);
                    TVs[args.Sig.Number - 10].MuteStateChanged(_SimplWindowsComms.BooleanOutput[args.Sig.Number].BoolValue);
                    break;
            }
        }

        public void WakeSystem()
        {
            foreach (Room room in rooms)
            {
                if (room.GetSources().Length > 1)
                    room.SetSourceSelected(0);
            }
        }

        public void SendMessage(string message)
        {
            _SimplWindowsComms.StringInput[1].StringValue = message;
        }

        void _ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }
        void _ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }
        void _ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
        public void PushSky1Button(int btnNum)
        {
            try
            {
                switch (btnNum)
                {
                    case 8: _sky1_IRPort.PressAndRelease("TV_GUIDE", 25); break;
                    //case 0: _sky1_IRPort.PressAndRelease("SKY", 25); break;
                    //case 2: _sky1_IRPort.PressAndRelease("I", 25); break;
                    case 7: _sky1_IRPort.PressAndRelease("BOX_OFFICE", 25); break;
                    case 9: _sky1_IRPort.PressAndRelease("1", 25); break;
                    case 10: _sky1_IRPort.PressAndRelease("2", 25); break;
                    case 11: _sky1_IRPort.PressAndRelease("3", 25); break;
                    case 19: _sky1_IRPort.PressAndRelease("RED", 25); break;
                    case 12: _sky1_IRPort.PressAndRelease("4", 25); break;
                    case 13: _sky1_IRPort.PressAndRelease("5", 25); break;
                    case 14: _sky1_IRPort.PressAndRelease("6", 25); break;
                    case 20: _sky1_IRPort.PressAndRelease("GREEN", 25); break;
                    case 15: _sky1_IRPort.PressAndRelease("7", 25); break;
                    case 16: _sky1_IRPort.PressAndRelease("8", 25); break;
                    case 17: _sky1_IRPort.PressAndRelease("9", 25); break;
                    case 21: _sky1_IRPort.PressAndRelease("YELLOW", 25); break;
                    case 18: _sky1_IRPort.PressAndRelease("0", 25); break;
                    case 22: _sky1_IRPort.PressAndRelease("BLUE", 25); break;
                    case 0: _sky1_IRPort.PressAndRelease("UP", 25); break;
                    case 1: _sky1_IRPort.PressAndRelease("LEFT", 25); break;
                    case 2: _sky1_IRPort.PressAndRelease("SELECT", 25); break;
                    case 3: _sky1_IRPort.PressAndRelease("RIGHT", 25); break;
                    case 4: _sky1_IRPort.PressAndRelease("DOWN", 25); break;
                    case 5: _sky1_IRPort.PressAndRelease("CH+", 25); break;
                    case 6: _sky1_IRPort.PressAndRelease("CH-", 25); break;
                    case 25: _sky1_IRPort.PressAndRelease("REV", 25); break;
                    case 26: _sky1_IRPort.PressAndRelease("PLAY", 25); break;
                    case 27: _sky1_IRPort.PressAndRelease("STOP", 25); break;
                    case 28: _sky1_IRPort.PressAndRelease("RECORD", 25); break;
                    case 29: _sky1_IRPort.PressAndRelease("FFWD", 25); break;
                    case 30: _sky1_IRPort.PressAndRelease("BACK_UP", 25); break;
                    case 31: _sky1_IRPort.PressAndRelease("PAUSE", 25); break;
                }

                logger.WriteLine("IR command sent on port 1");
            }
            catch (Exception ex)
            {
                logger.WriteLine("Problem in Sky: " + ex);
            }
        }
    }
}