using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using Independentsoft.Exchange;
using System.Collections.Generic;

namespace MaslowsMain
{
    public class MasterIpad
    {
        short _tpID;
        
        ControlSystem _cs;
        List<Room> _roomList;
        CrestronGo _iPad;

        int currentRoom { get; set; }


        public MasterIpad(CrestronGo iPad, List<Room> rooms, ControlSystem cs, short tpID)
        {
            _tpID = tpID;
            _cs = cs;
            _roomList = rooms;
            _iPad = iPad;

            _iPad.ParameterProjectName.Value = "ipadtest";
            _iPad.SigChange += MasterIPad_SigChange;
            _iPad.OnlineStatusChange += MasterIPad_OnlineStatusChange;
            if (_iPad.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                _cs.logger.WriteLine("master iPad not registered successfully");
            }
            currentRoom = 0;

            SubscribeToRoomEvents();
        }

        void SubscribeToRoomEvents()
        {
            _roomList[currentRoom].RoomTVConnectedEvent += MasterIpad_RoomTVConnectedEvent;
            _roomList[currentRoom].RoomVolChangedEvent += MasterIpad_RoomVolChangedEvent;
            _roomList[currentRoom].SourceSelectedEvent += MasterIpad_SourceSelectedEvent;
        }

        void UnsubscribeRoomEvents()
        {
            _roomList[currentRoom].RoomTVConnectedEvent -= MasterIpad_RoomTVConnectedEvent;
            _roomList[currentRoom].RoomVolChangedEvent -= MasterIpad_RoomVolChangedEvent;
            _roomList[currentRoom].SourceSelectedEvent -= MasterIpad_SourceSelectedEvent;
        }

        private void MasterIpad_SourceSelectedEvent(short source)
        {
            AddSourceFb(source);
        }
        private void MasterIpad_RoomVolChangedEvent(int vol)
        {
            _iPad.StringInput[15].StringValue = vol + "%";
        }
        private void MasterIpad_RoomTVConnectedEvent(bool connected)
        {
            if(connected)
            {
                _iPad.BooleanInput[101].BoolValue = false;
                _iPad.BooleanInput[111].BoolValue = true;
            }
            else
            {
                _iPad.BooleanInput[101].BoolValue = true;
                _iPad.BooleanInput[111].BoolValue = false;
            }
        }
        private void MasterIpad_RoomIPTVConnectedEvent(bool connected)
        {
            if (connected)
            {
                _iPad.BooleanInput[102].BoolValue = false;
                _iPad.BooleanInput[112].BoolValue = true;
            }
            else
            {
                _iPad.BooleanInput[102].BoolValue = true;
                _iPad.BooleanInput[112].BoolValue = false;
            }
        }
        private void MasterIPad_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (currentDevice == _iPad)
            {
                if (args.DeviceOnLine)
                {
                    _cs.logger.WriteLine("iPad Connected");
                    InitializeRoomData();
                }
                else
                {
                    _cs.logger.WriteLine("iPad Offline");
                    _roomList[currentRoom].DisconnectRoomEquipment(_tpID);
                }
            }
        }
        private void MasterIPad_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.NA:
                    break;
                case eSigType.Bool:
                    {
                        if (args.Sig.Number >= 10 && args.Sig.Number <= 20)
                        {
                            if (args.Sig.BoolValue == true)
                            {
                                UnsubscribeRoomEvents();

                                _roomList[currentRoom].DisconnectRoomEquipment(_tpID);
                                _iPad.BooleanInput[50].BoolValue = false;
                                currentRoom = int.Parse(args.Sig.Number.ToString()) - 10;
                                AddRoomFb(currentRoom);

                                SubscribeToRoomEvents();
                                InitializeRoomData();
                            }
                        }
                        if (args.Sig.Number == 30) if (args.Sig.BoolValue == true) _roomList[currentRoom].VolDown();
                        if (args.Sig.Number == 31) if (args.Sig.BoolValue == true) _roomList[currentRoom].VolUp();
                        if (args.Sig.Number == 40) if (args.Sig.BoolValue == true) _roomList[currentRoom].SetSourceSelected(0);
                        if (args.Sig.Number == 41) if (args.Sig.BoolValue == true) _roomList[currentRoom].SetSourceSelected(1);
                        if (args.Sig.Number == 42) if (args.Sig.BoolValue == true) _roomList[currentRoom].SetSourceSelected(2);
                        if (args.Sig.Number == 43) if (args.Sig.BoolValue == true) _roomList[currentRoom].SetSourceSelected(-1);

                        CheckSourcePresses(args);
                            break;
                    }
            }
        }

        void InitializeRoomData()
        {
            _roomList[currentRoom].ConnectRoomEquipment(_tpID);
            _iPad.StringInput[10].StringValue = _roomList[currentRoom].GetRoomName();
            _iPad.StringInput[15].StringValue = _roomList[currentRoom].GetRoomVolLevel().ToString() + "%";
            AddRoomFb(currentRoom);
            PopulateSources();
            AddSourceFb(_roomList[currentRoom].GetSourceSelected());
        }
        void PopulateSources()
        {
            string[] roomSources = _roomList[currentRoom].GetSources();
            if(roomSources.Length == 1)
            {
                _iPad.BooleanInput[60].BoolValue = true;
                _iPad.StringInput[40].StringValue = roomSources[0];
                _iPad.BooleanInput[61].BoolValue = false;
                _iPad.BooleanInput[62].BoolValue = false;
            }
            if (roomSources.Length == 2)
            {
                _iPad.BooleanInput[60].BoolValue = true;
                _iPad.StringInput[40].StringValue = roomSources[0];
                _iPad.BooleanInput[61].BoolValue = true;
                _iPad.StringInput[41].StringValue = roomSources[1];
                _iPad.BooleanInput[62].BoolValue = false;
            }
            if (roomSources.Length == 3)
            {
                _iPad.BooleanInput[60].BoolValue = true;
                _iPad.StringInput[40].StringValue = roomSources[0];
                _iPad.BooleanInput[61].BoolValue = true;
                _iPad.StringInput[41].StringValue = roomSources[1];
                _iPad.BooleanInput[62].BoolValue = true;
                _iPad.StringInput[42].StringValue = roomSources[2];
            }
        }
        void AddSourceFb(short source)
        {
            if (source == -1)
            {
                _iPad.BooleanInput[40].BoolValue = false;
                _iPad.BooleanInput[41].BoolValue = false;
                _iPad.BooleanInput[42].BoolValue = false;

                _iPad.BooleanInput[43].BoolValue = true;
                _iPad.BooleanInput[50].BoolValue = false;

                return;
            }

            if (source == 0)
            {
                _iPad.BooleanInput[40].BoolValue = true;
                _iPad.BooleanInput[41].BoolValue = false;
                _iPad.BooleanInput[42].BoolValue = false;

                _iPad.BooleanInput[43].BoolValue = false;
            }
            if (source == 1)
            {
                _iPad.BooleanInput[40].BoolValue = false;
                _iPad.BooleanInput[41].BoolValue = true;
                _iPad.BooleanInput[42].BoolValue = false;

                _iPad.BooleanInput[43].BoolValue = false;
            }
            if (source == 2)
            {
                _iPad.BooleanInput[40].BoolValue = false;
                _iPad.BooleanInput[41].BoolValue = false;
                _iPad.BooleanInput[42].BoolValue = true;

                _iPad.BooleanInput[43].BoolValue = false;
            }

            string[] roomSources = _roomList[currentRoom].GetSources();

            if (roomSources[source] == "IPTV")
                _iPad.BooleanInput[50].BoolValue = true;
            else
                _iPad.BooleanInput[50].BoolValue = false;
        }
        void AddRoomFb(int roomNum)
        {
            for(int i = 0; i < _roomList.Count; i++)
            {
                _iPad.BooleanInput[(uint)i + 10].BoolValue = false;
                if(roomNum == i)
                    _iPad.BooleanInput[(uint)i + 10].BoolValue = true;
            }
        }

        void CheckSourcePresses(SigEventArgs args)
        {
            if (args.Sig.Number == 200) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(7);
            if (args.Sig.Number == 201) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(8);
            if (args.Sig.Number == 202) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 203) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 204) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 205) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);


            if (args.Sig.Number == 206) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(9);
            if (args.Sig.Number == 207) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(10);
            if (args.Sig.Number == 208) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(11);
            if (args.Sig.Number == 209) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(12);
            if (args.Sig.Number == 210) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(13);
            if (args.Sig.Number == 211) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(14);
            if (args.Sig.Number == 212) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(15);
            if (args.Sig.Number == 213) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(16);
            if (args.Sig.Number == 214) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(17);
            if (args.Sig.Number == 215) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(18);


            if (args.Sig.Number == 216) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(0);
            if (args.Sig.Number == 217) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(4);
            if (args.Sig.Number == 218) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(1);
            if (args.Sig.Number == 219) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(3);
            if (args.Sig.Number == 232) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(2);


            if (args.Sig.Number == 220) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(19);
            if (args.Sig.Number == 221) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(20);
            if (args.Sig.Number == 222) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(21);
            if (args.Sig.Number == 223) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(22);


            if (args.Sig.Number == 224) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(5);
            if (args.Sig.Number == 225) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(6);


            if (args.Sig.Number == 226) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 227) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 228) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 229) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 230) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
            if (args.Sig.Number == 231) if (args.Sig.BoolValue == true) _roomList[currentRoom].SourceBtnPressed(111);
        }
    }
}
