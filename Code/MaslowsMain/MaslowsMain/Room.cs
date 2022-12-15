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
        public event Action<bool> RoomIPTVConnectedEvent;
        public event Action<int> RoomVolChangedEvent;

        public Room(short roomID, IPTV iptv, LGTV lgtv, ControlSystem cs)
        {
            _cs = cs;
            _roomID = roomID;

            this.iptv = iptv;
            iptv.IPTVConnectedEvent += Iptv_IPTVConnectedEvent;
            
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

        private void Lgtv_VolChangeEvent(int volLevel)
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

        private void Lgtv_TVConnectedEvent(bool connStatus)
        {
            if(this.RoomTVConnectedEvent != null)
            {
                this.RoomTVConnectedEvent(connStatus);
            }
        }

        private void Iptv_IPTVConnectedEvent(bool connStatus)
        {
            if (this.RoomIPTVConnectedEvent != null)
            {
                this.RoomIPTVConnectedEvent(connStatus);
            }
        }

        public short GetRoomID() => _roomID;

        public string GetRoomName() => _settings.roomName;
        public string[] GetSources() => _settings.sources;
        public short GetSourceSelected() => _settings.sourceSelected;
        public short GetNeighbourRoom() => _settings.neighbourRoom;
        public int GetRoomVolLevel() => lgtv.GetVolumeLevel();
        public int GetEventSubscribersCount()
        {
            if (SourceSelectedEvent != null)
                return SourceSelectedEvent.GetInvocationList().Length;
            else
                return 0;
        }
        public void SetSourceSelected(short value, int tpid)
        {
            try
            {
                try
                {
                    _settings.sourceSelected = value;
                    FileOperations.UpdateSettings(_roomID.ToString(), _settings);

                    OnSourceSelected(tpid);
                }
                catch (Exception ex) { _cs.logger.WriteLine("Problem in Room.SetSourceSelected(): " + ex); }
            }
            catch (Exception ex)
            {
                _cs.logger.WriteLine("Problem in Room.SetSourceSelected(): " + ex);
            }
        }

        public void OnSourceSelected(int tpID)
        {
            if (_settings.sourceSelected != -1)
                lgtv.SourceSelectedChanged(_settings.sources[_settings.sourceSelected], tpID);
            else
                lgtv.SourceSelectedChanged("Off", tpID);

            if (this.SourceSelectedEvent != null)
            {
                this.SourceSelectedEvent(GetSourceSelected());
            }
        }

        public void SourceBtnPressed(int btnPressed)
        {
            if (_settings.sources[_settings.sourceSelected] == "IPTV")
            {
                iptv.PushButton(btnPressed);
            }
        }
        public void VolUp()
        {
            lgtv.VolUp();
        }
        public void VolDown()
        {
            lgtv.VolUp();
        }

        public void ConnectRoomEquipment(int tpID)
        {
            lgtv.ConnectRequest(tpID);
        }
        public void DisconnectRoomEquipment(int tpID)
        {
            lgtv.Disconnect(tpID);
        }

    }
}
