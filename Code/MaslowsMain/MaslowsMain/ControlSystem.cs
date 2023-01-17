using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;            // For Threadingb
using Crestron.SimplSharpPro.UI;

namespace MaslowsMain
{
    public class ControlSystem : CrestronControlSystem
    {
        //Hardware
        Relay fireAlarmRelay;

        MasterIpad masteriPadController;
        CrestronGo masterIPad;
        
        public List<Room> rooms;
        public ConsoleLogger logger;
        public Touchpannel[] tp;
        public Touchpannel tpDecider;
        public IPTV[] iptvs;
        public LGTV[] TVs;

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


                if (this.SupportsEthernet)
                {
                    logger = new ConsoleLogger(55555, this);

                    InitializeEquipment();
                    InitializeRooms();
                    InitializeTPs();
                }
                if (this.SupportsRelay)
                {
                    fireAlarmRelay = this.RelayPorts[1];
                    fireAlarmRelay.StateChange += FireAlarmRelay_StateChange;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        private void FireAlarmRelay_StateChange(Relay relay, RelayEventArgs args)
        {
            for (int i = 0; i < tp.Length; i++)
                tp[i].OnFireAlarmStateChange(args.State);
        }
        public void FireAlarmState(bool state)
        {
            for (int i = 0; i < tp.Length; i++)
                tp[i].OnFireAlarmStateChange(state); 


            for (int i = 0; i < 10; i++) //num of amps
            {
                for (int j = 1; j < 5; j++) //num of channels per amp
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://10.0.1." + (20 + i) + "/rest-api/settings/channel/" + j + "/dsp/mute");
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "PUT";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"value\": " + state.ToString().ToLower() + "}";

                        streamWriter.Write(json);
                        logger.WriteLine("KArray Amp IP: " + "http://10.0.1." + (20 + i) + ",Sending message: " + json.Replace("{", "(").Replace("}", ")"));
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        result = result.Replace('{', '(');
                        result = result.Replace('}', ')');
                        logger.WriteLine(result.ToString());
                    }
                }
            }
        }

        void InitializeEquipment()
        {
            try
            {
                iptvs = new IPTV[12];
                TVs = new LGTV[12];

                for (int i = 60; i < 72; i++)
                {
                    iptvs[i - 60] = new IPTV(
                        "172.16.30." + i.ToString(),
                        7070,
                        "iptv" + (i - 59).ToString(),
                        this
                        );
                }

                TVs[0] = new LGTV(this, "TV1", "172.16.30.80", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[1] = new LGTV(this, "TV2", "172.16.30.81", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[2] = new LGTV(this, "TV3", "172.16.30.82", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[3] = new LGTV(this, "TV4", "172.16.30.83", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[4] = new LGTV(this, "TV5", "172.16.30.84", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[5] = new LGTV(this, "TV6", "172.16.30.85", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[6] = new LGTV(this, "TV7", "172.16.30.86", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[7] = new LGTV(this, "TV8", "172.16.30.87", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[8] = new LGTV(this, "TV9", "172.16.30.88", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[9] = new LGTV(this, "TV10", "172.16.30.89", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[10] = new LGTV(this, "TV11", "172.16.30.90", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
                TVs[11] = new LGTV(this, "TV12", "172.16.30.91", 9761, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
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
                rooms.Add(new Room(1, iptvs[0], TVs[0], this));
                rooms.Add(new Room(2, iptvs[1], TVs[1], this));
                rooms.Add(new Room(3, iptvs[2], TVs[2], this));
                rooms.Add(new Room(4, iptvs[3], TVs[3], this));
                rooms.Add(new Room(5, iptvs[4], TVs[4], this));
                rooms.Add(new Room(6, iptvs[5], TVs[5], this));
                rooms.Add(new Room(7, iptvs[6], TVs[6], this));
                rooms.Add(new Room(8, iptvs[7], TVs[7], this));
                rooms.Add(new Room(9, iptvs[8], TVs[8], this));
                rooms.Add(new Room(10, iptvs[9], TVs[9], this));
                rooms.Add(new Room(11, iptvs[10], TVs[10], this));
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

            tpDecider = new Touchpannel(50000, rooms[0], this);

            tp = new Touchpannel[8];
            tp[0] = new Touchpannel(TOUCHPANNEL_START_PORT + 1, rooms[0], this);
            tp[1] = new Touchpannel(TOUCHPANNEL_START_PORT + 2, rooms[1], this);
            tp[2] = new Touchpannel(TOUCHPANNEL_START_PORT + 3, rooms[3], this);
            tp[3] = new Touchpannel(TOUCHPANNEL_START_PORT + 4, rooms[5], this);
            tp[4] = new Touchpannel(TOUCHPANNEL_START_PORT + 5, rooms[6], this);
            tp[5] = new Touchpannel(TOUCHPANNEL_START_PORT + 6, rooms[8], this);
            tp[6] = new Touchpannel(TOUCHPANNEL_START_PORT + 7, rooms[9], this);
            tp[7] = new Touchpannel(TOUCHPANNEL_START_PORT + 8, rooms[10], this);
        }

        public override void InitializeSystem()
        {
            try
            {
                for (int i = 0; i < tp.Length; i++)
                {
                    tp[i].Start();
                }
                tpDecider.Start();
                logger.Start();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
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
    }
}