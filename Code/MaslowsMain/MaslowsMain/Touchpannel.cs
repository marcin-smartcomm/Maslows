using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Timers;
using WebsocketServer;

namespace MaslowsMain
{

    public class Touchpannel
    {
        int tpID;
        Room currentRoom;

        public ControlSystem controlSystem;

        private static Timer aTimer;

        private WebsocketSrvr CommsServer;
        private bool _clientConnected;

        private List<string> _backlog;
        bool isPinging = false;

        public Touchpannel(int port, Room currentRoom, ControlSystem cs)
        {
            try
            {
                controlSystem = cs;

                tpID = port - 50000;
                this.currentRoom = currentRoom;
                SubscribeToRoomEvents();

                CommsServer = new WebsocketSrvr();
                CommsServer.Initialize(port);
                CommsServer.OnClientConnectedChange += OnClientConnected;
                CommsServer.OnStringSignalChange += OnReceivingMessage;

                _backlog = new List<string>();

                _clientConnected = false;

                aTimer = new Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 59000;
                aTimer.Enabled = true;
            }
            catch (Exception e)
            {
                controlSystem.logger.WriteLine("TP Constructor issue: \n" + e.ToString());
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
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

        void SubscribeToRoomEvents()
        {
            currentRoom.SourceSelectedEvent += OnSourceSelected;
            currentRoom.RoomTVConnectedEvent += CurrentRoom_RoomTVConnectedEvent;
            currentRoom.RoomIPTVConnectedEvent += CurrentRoom_RoomIPTVConnectedEvent;
            currentRoom.RoomVolChangedEvent += CurrentRoom_RoomVolChangedEvent;
        }

        private void CurrentRoom_RoomVolChangedEvent(int volLevel)
        {
            CommsServer.SetIndirectTextSignal(1, "Volume " + volLevel);
        }

        private void CurrentRoom_RoomIPTVConnectedEvent(bool connected)
        {
            if(connected)
                CommsServer.SetIndirectTextSignal(1, "IPTV Connected");
            else
                CommsServer.SetIndirectTextSignal(1, "IPTV Disconnected");
        }

        private void CurrentRoom_RoomTVConnectedEvent(bool connected)
        {
            if (connected)
                CommsServer.SetIndirectTextSignal(1, "TV Connected");
            else
                CommsServer.SetIndirectTextSignal(1, "TV Disconnected");
        }

        void UnsubscribeFromRoomEvents()
        {
            currentRoom.SourceSelectedEvent -= OnSourceSelected;
            currentRoom.RoomTVConnectedEvent -= CurrentRoom_RoomTVConnectedEvent;
            currentRoom.RoomIPTVConnectedEvent -= CurrentRoom_RoomIPTVConnectedEvent;
            currentRoom.RoomVolChangedEvent -= CurrentRoom_RoomVolChangedEvent;
        }
        public void Start()
        {
            CommsServer.StartServer();
        }

        public void Stop()
        {
            CommsServer.StopServer();
        }

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

        private void OnClientConnected(ushort state)
        {
            if (state == 0)
            {
                // Disconnected
                _clientConnected = false;
                currentRoom.DisconnectRoomEquipment(tpID);
            }
            else
            {
                // Connected
                _clientConnected = true;
                CommsServer.SetIndirectTextSignal(1, "\n-- CONNECTED --\n");

                if (_backlog.Count > 0)
                {
                    foreach (var msg in _backlog)
                    {
                        CommsServer.SetIndirectTextSignal(1, msg);
                    }
                }

                _backlog.Clear();
            }
        }

        private void OnReceivingMessage(ushort state, SimplSharpString value)
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
            CommsServer.SetIndirectTextSignal(1, "NeighbourRoom " + currentRoom.GetNeighbourRoom());
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
                else if (incomingRequest.Contains("ConnectEquipment")) currentRoom.ConnectRoomEquipment(tpID);
                else if (incomingRequest.Contains("DisconnectEquipment")) currentRoom.DisconnectRoomEquipment(tpID);
                else if(incomingRequest.Contains("Volume"))
                {
                    if (incomingRequest.Split(':')[1].Equals("+"))
                        currentRoom.VolUp();
                    else if (incomingRequest.Split(':')[1].Equals("-"))
                        currentRoom.VolDown();
                }
            }
            catch(Exception ex)
            {
                controlSystem.logger.WriteLine("Problem in Touchpannel.evaluateString: " + ex);
            }
        }
    }
}
