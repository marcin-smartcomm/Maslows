using System;
using System.Threading;
using System.Threading.Tasks;
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
        public event Action<bool> RoomMuteStateChangedEvent;
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
            lgtv.MuteStateChangedEvent += Lgtv_MuteStateChangedEvent;
            lgtv.TVSelectedEvent += Lgtv_TVSelectedEvent;

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

        private void Lgtv_MuteStateChangedEvent(bool newMuteState)
        {
            SetNewMuteState(newMuteState);
        }

        private void Lgtv_TVSelectedEvent(bool obj)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                iptv.PushButton(10);
                Thread.Sleep(1000);
                iptv.PushButton(15);
            });
        }

        void Lgtv_VolChangeEvent(int volLevel)
        {
            SetNewVolLevel(volLevel);
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
        public int GetRoomVolLevel() => _settings.volume;
        public bool GetMuteState() => _settings.muteState;
        public bool GetJoinedState() => _settings.joined;
        public bool GetMasterPanel() => _settings.MasterPanel;
        public bool GetSlavePanel() => _settings.slave;

        public void SetNewMuteState(bool newMuteState)
        {
            try
            {
                _settings.muteState = newMuteState;
                FileOperations.UpdateSettings(_roomID.ToString(), _settings);
            }
            catch (Exception ex)
            {
                _cs.logger.WriteLine("Problem in Room.Lgtv_MuteStateChangedEvent(): " + ex);
            }

            if (RoomMuteStateChangedEvent != null)
            {
                RoomMuteStateChangedEvent(newMuteState);
            }
        }

        public void SetNewVolLevel(int volLevel)
        {
            try
            {
                _settings.volume = volLevel;
                FileOperations.UpdateSettings(_roomID.ToString(), _settings);
            }
            catch (Exception ex)
            {
                _cs.logger.WriteLine("Problem in Room.Lgtv_VolChangeEvent(): " + ex);
            }

            if (this.RoomVolChangedEvent != null)
            {
                this.RoomVolChangedEvent(volLevel);
            }
        }

        public void SetSourceSelected(short value)
        {
            try
            {
                try
                {
                    _settings.sourceSelected = value;
                    FileOperations.UpdateSettings(_roomID.ToString(), _settings);

                    if(!_settings.slave && _settings.joined)
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
            if (_settings.sources[_settings.sourceSelected] == "TV")
            {
                try
                {
                    iptv.PushButton(btnPressed);
                    if (!_settings.slave && _settings.joined)
                        _cs.rooms[_settings.neighbourRoom].SourceBtnPressed(btnPressed);
                }catch(Exception ex)
                {
                    _cs.logger.WriteLine("Problem in Room.SourceBtnPressed: " + ex.ToString());
                }
            }
            if(_settings.sources[_settings.sourceSelected] == "Sky")
            {
                _cs.PushSky1Button(btnPressed);
            }
        }

        public void SkyBtnPressed(int btnNum)
        {
            _cs.PushSky1Button(btnNum);
        }
        public void VolUp()
        {
            _cs.logger.WriteLine(GetRoomName() + ": Vol+");
            lgtv.VolUp();
            if (!_settings.slave && _settings.joined)
                _cs.rooms[_settings.neighbourRoom].VolUp();
        }
        public void VolDown()
        {
            _cs.logger.WriteLine(GetRoomName() + ": Vol-");
            lgtv.VolDown();
            if (!_settings.slave && _settings.joined)
                _cs.rooms[_settings.neighbourRoom].VolDown();
        }

        public void Mute()
        {
            _cs.logger.WriteLine(GetRoomName() + ": Mute");
            lgtv.Mute();
            if (!_settings.slave && _settings.joined)
                _cs.rooms[_settings.neighbourRoom].Mute();
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
