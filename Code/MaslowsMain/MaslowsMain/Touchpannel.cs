using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WebsocketServer;

namespace MaslowsMain
{

    public class Touchpannel
    {
        byte tpID;
        Room currentRoom;

        public ControlSystem controlSystem;

        private static System.Timers.Timer aTimer;

        private WebsocketSrvr CommsServer;
        private bool _clientConnected;

        private List<string> _backlog;
        bool isPinging = false;

        public Touchpannel(int port, Room currentRoom, ControlSystem cs)
        {
            try
            {
                controlSystem = cs;

                tpID = (byte)(port - 50000);
                this.currentRoom = currentRoom;
                SubscribeToRoomEvents();

                CommsServer = new WebsocketSrvr();
                CommsServer.Initialize(port);
                CommsServer.OnClientConnectedChange += OnClientConnected;
                CommsServer.OnStringSignalChange += OnReceivingMessage;

                _backlog = new List<string>();

                _clientConnected = false;

                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 59000;
                aTimer.Enabled = true;
            }
            catch (Exception e)
            {
                controlSystem.logger.WriteLine("TP Constructor issue: \n" + e.ToString());
            }
        }

        void SubscribeToRoomEvents()
        {
            currentRoom.SourceSelectedEvent += OnSourceSelected;
            currentRoom.RoomTVConnectedEvent += CurrentRoom_RoomTVConnectedEvent;
            currentRoom.RoomVolChangedEvent += CurrentRoom_RoomVolChangedEvent;
            currentRoom.RoomsJoinedEvent += CurrentRoom_RoomsJoinedEvent;
            currentRoom.SlaveModeEvent += CurrentRoom_SlaveModeEvent;
        }

        void UnsubscribeFromRoomEvents()
        {
            currentRoom.SourceSelectedEvent -= OnSourceSelected;
            currentRoom.RoomTVConnectedEvent -= CurrentRoom_RoomTVConnectedEvent;
            currentRoom.RoomVolChangedEvent -= CurrentRoom_RoomVolChangedEvent;
            currentRoom.RoomsJoinedEvent -= CurrentRoom_RoomsJoinedEvent;
            currentRoom.SlaveModeEvent -= CurrentRoom_SlaveModeEvent;
        }

        private void CurrentRoom_RoomsJoinedEvent(bool state)
        {
            CommsServer.SetIndirectTextSignal(1, "JoinedState " + state);
            CommsServer.SetIndirectTextSignal(1, "MasterPanel " + state);
        }

        private void CurrentRoom_SlaveModeEvent(bool state)
        {
            CommsServer.SetIndirectTextSignal(1, "SlavePanel " + state);
        }

        void CurrentRoom_RoomVolChangedEvent(int volLevel)
        {
            CommsServer.SetIndirectTextSignal(1, "Volume " + volLevel);
        }
        void CurrentRoom_RoomTVConnectedEvent(bool connected)
        {
            if (connected)
                CommsServer.SetIndirectTextSignal(1, "TV Connected");
            else
                CommsServer.SetIndirectTextSignal(1, "TV Disconnected");
        }

        void SendAvailableLocations()
        {
            string toSend = "";

            for(int i  = 0; i < TPLocations.availableLocations.Count(); i++)
            {
                if (TPLocations.availableLocationsIndex[i] == true)
                {
                    if (i == TPLocations.availableLocations.Count() - 1)
                        toSend += TPLocations.availableLocations[i] + ":" + (i+1);
                    else
                    {
                        if((i+1) == TPLocations.availableLocations.Count() - 1 && TPLocations.availableLocationsIndex[i+1] == false)
                            toSend += TPLocations.availableLocations[i] + ":" + (i + 1);
                        else
                            toSend += TPLocations.availableLocations[i] + ":" + (i + 1) + "|";
                    }
                }
            }

            CommsServer.SetIndirectTextSignal(1, "AvailableLocations " + toSend);
            controlSystem.logger.WriteLine(toSend);
        }
        void SendSources()
        {
            string[] roomSources = currentRoom.GetSources();
            if (roomSources != null)
            {
                string toReturn = "Sources ";
                foreach (string source in roomSources)
                {
                    if (source == roomSources[roomSources.Length - 1])
                    {
                        toReturn += source;
                    }
                    else
                    {
                        toReturn += source + ":";
                    }
                }
                CommsServer.SetIndirectTextSignal(1, toReturn);
            }
        }
        void SendRoomName()
        {
            CommsServer.SetIndirectTextSignal(1, "RoomName " + currentRoom.GetRoomName());
        }
        void SendSourceSelected()
        {
            CommsServer.SetIndirectTextSignal(1, "SourceSelected " + currentRoom.GetSourceSelected());
        }
        void SendNeihbourRoom()
        {
            CommsServer.SetIndirectTextSignal(1, "NeighbourRoom " + currentRoom.GetNeighbourRoom() + ":" + 
                controlSystem.rooms[currentRoom.GetNeighbourRoom()].GetRoomName());
        }

        void OnClientConnected(ushort state)
        {
            if (state == 0)
            {
                // Disconnected
                _clientConnected = false;
                currentRoom.DisconnectRoomEquipment(tpID);
                if (tpID > 0)
                {
                    if (TPLocations.availableLocations.IndexOf(currentRoom.GetRoomName()) != -1)
                        TPLocations.availableLocationsIndex[TPLocations.availableLocations.IndexOf(currentRoom.GetRoomName())] = true;

                    if (currentRoom.GetRoomName().Equals("Meeting Room 402"))
                        TPLocations.availableLocationsIndex[TPLocations.availableLocations.IndexOf("Meeting Room 401")] = true;

                    if (currentRoom.GetRoomName().Equals("Meeting Room 302"))
                        TPLocations.availableLocationsIndex[TPLocations.availableLocations.IndexOf("Meeting Room 301")] = true;
                }
            }
            else
            {
                // Connected
                _clientConnected = true;
                CommsServer.SetIndirectTextSignal(1, "\n-- CONNECTED --\n");
                if (tpID > 0)
                {
                    if (TPLocations.availableLocations.IndexOf(currentRoom.GetRoomName()) != -1)
                        TPLocations.availableLocationsIndex[TPLocations.availableLocations.IndexOf(currentRoom.GetRoomName())] = false;

                    if (currentRoom.GetRoomName().Equals("Meeting Room 402"))
                        TPLocations.availableLocationsIndex[TPLocations.availableLocations.IndexOf("Meeting Room 401")] = false;

                    if (currentRoom.GetRoomName().Equals("Meeting Room 302"))
                        TPLocations.availableLocationsIndex[TPLocations.availableLocations.IndexOf("Meeting Room 301")] = false;

                    Task.Run(() =>
                    {
                        Thread.Sleep(3000);
                        currentRoom.DisconnectRoomEquipment(tpID);
                    });
                }

                if (_backlog.Count > 0)
                {
                    foreach (var msg in _backlog)
                    {
                        CommsServer.SetIndirectTextSignal(1, msg);
                    }
                }

                _backlog.Clear();

                if (tpID == 0)
                    SendAvailableLocations();
            }
        }
        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (!isPinging)
            {
                Stop();
                Start();
            }
            isPinging = false;
        }
        void OnSourceSelected(short newSource)
        {
            SendSourceSelected();
        }
        void OnReceivingMessage(ushort state, SimplSharpString value)
        {
            controlSystem.logger.WriteLine(value.ToString());
            if (value.ToString() == "__ping__")
            {
                isPinging = true;
                // _logger.WriteLine("panel is pinging server, isPinging = "+isPinging.ToString());
                CommsServer.SetIndirectTextSignal(1, "__pong__");
            }
            else
            {
                evaluateString(value.ToString());
            }
        }

        void evaluateString(string incomingRequest)
        {
            try
            {
                if (incomingRequest.Contains("GetRoomName")) SendRoomName();
                else if (incomingRequest.Contains("GetSources")) SendSources();
                else if (incomingRequest.Contains("GetSourceSelected")) SendSourceSelected();
                else if (incomingRequest.Contains("GetNeighbourRoom")) SendNeihbourRoom();
                else if (incomingRequest.Contains("GetVolumeLevel")) CommsServer.SetIndirectTextSignal(1, "Volume " + currentRoom.GetRoomVolLevel());
                else if (incomingRequest.Contains("GetJoinedState")) CommsServer.SetIndirectTextSignal(1, "JoinedState " + currentRoom.GetJoinedState());
                else if (incomingRequest.Contains("MasterPanel")) CommsServer.SetIndirectTextSignal(1, "MasterPanel " + currentRoom.GetMasterPanel());
                else if (incomingRequest.Contains("SlavePanel")) CommsServer.SetIndirectTextSignal(1, "SlavePanel " + currentRoom.GetSlavePanel());
                else if (incomingRequest.Contains("SetSourceSelected")) currentRoom.SetSourceSelected(short.Parse(incomingRequest.Split(':')[1]));
                else if (incomingRequest.Contains("RoomChange"))
                {
                    UnsubscribeFromRoomEvents();
                    currentRoom = controlSystem.rooms[int.Parse(incomingRequest.Split(':')[1]) - 1];
                    SubscribeToRoomEvents();

                    CommsServer.SetIndirectTextSignal(1, "RoomChanged");
                }
                else if (incomingRequest.Contains("RoomOff"))
                {
                    currentRoom.SetSourceSelected(-1);
                    CommsServer.SetIndirectTextSignal(1, "SourceSelected " + currentRoom.GetSourceSelected());
                }
                else if (incomingRequest.Contains("SourceBtn"))
                {
                    int btnNum = int.Parse(incomingRequest.Split(':')[1]);
                    currentRoom.SourceBtnPressed(btnNum);
                }
                else if (incomingRequest.Contains("SkyBtn"))
                {
                    int btnNum = int.Parse(incomingRequest.Split(':')[1]);
                    currentRoom.SkyBtnPressed(btnNum);
                }
                else if (incomingRequest.Contains("ConnectEquipment")) currentRoom.ConnectRoomEquipment(tpID);
                else if (incomingRequest.Contains("DisconnectEquipment")) currentRoom.DisconnectRoomEquipment(tpID);
                else if (incomingRequest.Contains("Volume"))
                {
                    if (incomingRequest.Split(':')[1].Equals("+"))
                        currentRoom.VolUp();
                    else if (incomingRequest.Split(':')[1].Equals("-"))
                        currentRoom.VolDown();
                }
                else if (incomingRequest.Contains("JoinRooms")) currentRoom.JoinRooms();
                else if (incomingRequest.Contains("SeperateRooms")) currentRoom.SeperateRooms();
            }
            catch (Exception ex)
            {
                controlSystem.logger.WriteLine("Problem in Touchpannel.evaluateString: " + ex);
            }
        }

        public void Start()
        {
            CommsServer.StartServer();
        }
        public void Stop()
        {
            CommsServer.StopServer();
        }
        public void OnFireAlarmStateChange(bool state) => CommsServer.SetIndirectTextSignal(1, "FireAlarm " + state.ToString());
        public void WriteLine(string msg, params object[] args)
        {
            var text = String.Format(msg, args) + "\n";

            if (_clientConnected)
            {
                CommsServer.SetIndirectTextSignal(1, text);
            }
            else
            {
                _backlog.Add(text);
            }
        }
    }
}
