using System;
using Newtonsoft.Json;

namespace MaslowsMain
{
    public class Room
    {
        ControlSystem _cs;
        
        short _roomID;
        RoomSettings _settings;
        IPTV iptv;
        LGTV lgtv;

        public event Action<short> SourceSelectedEvent;
        public event Action<bool> RoomTVConnectedEvent;
        public event Action<int> RoomVolChangedEvent;
        public event Action<bool> RoomsJoinedEvent;
        public event Action<bool> SlaveModeEvent;

        public Room(short roomID, IPTV iptv, LGTV lgtv, ControlSystem cs)
        {
            _cs = cs;
            _roomID = roomID;

            this.iptv = iptv;
            
            this.lgtv = lgtv;
            lgtv.TVConnectedEvent += Lgtv_TVConnectedEvent;
            lgtv.VolChangeEvent += Lgtv_VolChangeEvent;

            try
            {
                _settings = FileOperations.loadJson(_roomID.ToString());
                this.lgtv.volLevel = _settings.volume;

                if(_settings.sourceSelected != -1)
                    this.lgtv.currentSource = _settings.sources[_settings.sourceSelected];
                else
                    this.lgtv.currentSource = "Off";
            }
            catch (Exception ex)
            {
                _cs.logger.WriteLine("Problem in Room" + _roomID + " Constructor " + ex);
            }
        }

        void Lgtv_VolChangeEvent(int volLevel)
        {
            try
            {
                _settings.volume = volLevel;
                FileOperations.UpdateSettings(_roomID.ToString(), _settings);
            }
            catch(Exception ex)
            {
                _cs.logger.WriteLine("Problem in Room.Lgtv_VolChangeEvent(): " + ex);
            }

            if (this.RoomVolChangedEvent != null)
            {
                this.RoomVolChangedEvent(volLevel);
            }
        }
        void Lgtv_TVConnectedEvent(bool connStatus)
        {
            if(this.RoomTVConnectedEvent != null)
            {
                this.RoomTVConnectedEvent(connStatus);
            }
        }
        void OnSourceSelected()
        {
            if (_settings.sourceSelected != -1)
                lgtv.SourceSelectedChanged(_settings.sources[_settings.sourceSelected]);
            else
                lgtv.SourceSelectedChanged("Off");

            if (this.SourceSelectedEvent != null)
            {
                this.SourceSelectedEvent(GetSourceSelected());
            }
        }

        void OnJoinedRooms(bool state)
        {
            if (this.RoomsJoinedEvent != null)
            {
                this.RoomsJoinedEvent(state);
            }
        }

        void OnSlaveMode(bool state)
        {
            if (this.SlaveModeEvent != null)
            {
                this.SlaveModeEvent(state);
            }
        }

        public string GetRoomName() => _settings.roomName;
        public string[] GetSources() => _settings.sources;
        public short GetSourceSelected() => _settings.sourceSelected;
        public short GetNeighbourRoom() => _settings.neighbourRoom;
        public int GetRoomVolLevel() => lgtv.GetVolumeLevel();
        public bool GetJoinedState() => _settings.joined;
        public bool GetMasterPanel() => _settings.MasterPanel;
        public bool GetSlavePanel() => _settings.slave;

        public void SetSourceSelected(short value)
        {
            try
            {
                try
                {
                    _settings.sourceSelected = value;
                    FileOperations.UpdateSettings(_roomID.ToString(), _settings);

                    if(!_settings.slave)
                    {
                        if(value == -1)
                        {
                            _cs.rooms[_settings.neighbourRoom].SetSourceSelected(value);
                        }
                        else
                        {
                            foreach (var source in _cs.rooms[_settings.neighbourRoom]._settings.sources)
                            {
                                if (source == _settings.sources[value])
                                {
                                    _cs.rooms[_settings.neighbourRoom].SetSourceSelected((short)Array.IndexOf(_cs.rooms[_settings.neighbourRoom]._settings.sources, source));
                                }
                            }
                        }
                    }

                    OnSourceSelected();
                }
                catch (Exception ex) { _cs.logger.WriteLine("Problem in Room.SetSourceSelected(): " + ex); }
            }
            catch (Exception ex)
            {
                _cs.logger.WriteLine("Problem in Room.SetSourceSelected(): " + ex);
            }
        }
        public void SourceBtnPressed(int btnPressed)
        {
            if (_settings.sources[_settings.sourceSelected] == "IPTV")
            {
                iptv.PushButton(btnPressed);
                if(!_settings.slave)
                    _cs.rooms[_settings.neighbourRoom].SourceBtnPressed(btnPressed);
            }
            if(_settings.sources[_settings.sourceSelected] == "Sky")
            {
                _cs.PushSky1Button(btnPressed);
            }
        }
        public void VolUp()
        {
            lgtv.VolUp();
            if (!_settings.slave)
                _cs.rooms[_settings.neighbourRoom].VolUp();
        }
        public void VolDown()
        {
            lgtv.VolDown();
            if (!_settings.slave)
                _cs.rooms[_settings.neighbourRoom].VolDown();
        }

        public void JoinRooms()
        {
            _settings.joined = true;
            _settings.MasterPanel = true;
            FileOperations.UpdateSettings(_roomID.ToString(), _settings);
            _cs.rooms[_settings.neighbourRoom].SlavePanelMode(true);

            OnJoinedRooms(true);
        }

        public void SeperateRooms()
        {
            _settings.joined = false;
            _settings.MasterPanel = false;
            FileOperations.UpdateSettings(_roomID.ToString(), _settings);
            _cs.rooms[_settings.neighbourRoom].SlavePanelMode(false);

            OnJoinedRooms(false);
        }

        public void SlavePanelMode(bool state)
        {
            _settings.slave = state;
            FileOperations.UpdateSettings(_roomID.ToString(), _settings);
            OnSlaveMode(state);
        }

        public void ConnectRoomEquipment(int tpID)
        {
            lgtv.ConnectRequest(tpID);
        }
        public void DisconnectRoomEquipment(int tpID)
        {
            //lgtv.Disconnect(tpID);
        }

    }
}
