using System;
using System.Collections.Generic;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;            // For Threading
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.EthernetCommunication;

namespace MaslowsMain
{
    public class ControlSystem : CrestronControlSystem
    {
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
                    logger = new ConsoleLogger(55555);

                    InitializeEquipment();
                    InitializeRooms();
                    InitializeTPs();
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        void InitializeEquipment()
        {
            try
            {
                iptvs = new IPTV[12];
                TVs = new LGTV[12];

                for (int i = 10; i < 22; i++)
                {
                    iptvs[i - 10] = new IPTV(
                        "192.168.1." + i.ToString(),
                        20060,
                        "iptv" + (i - 9).ToString(),
                        this
                        );
                }

                for (int i = 10; i < 22; i++)
                {
                    TVs[i - 10] = new LGTV(
                        this,
                        "LGTV" + (i - 9).ToString(),
                        "192.168.1." + i.ToString(),
                        9761
                        );
                }
            }
            catch(Exception ex)
            {
                logger.WriteLine("Problem in Initialize Equipment: " + ex.Message);
            }
        }
        void InitializeRooms()
        {
            rooms = new List<Room>();
            string[] sources = { "IPTV", "Mersive", "Laptop"};

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