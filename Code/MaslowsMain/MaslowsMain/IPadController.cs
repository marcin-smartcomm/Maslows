using Crestron.SimplSharpPro.AudioDistribution;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public class IPadController
    {
        Timer aTimer;
        bool holding, timerRunning;

        ControlSystem _cs;
        Room _room;
        CrestronGo _iPad;
        public IPadController(CrestronGo iPad, Room room, ControlSystem cs)
        {
            _cs = cs;
            _room = room;
            _iPad = iPad;

            _iPad.ParameterProjectName.Value = "iPad-Maslows-Room-v3";
            _iPad.SigChange += _iPad_SigChange;
            _iPad.OnlineStatusChange += _iPad_OnlineStatusChange; ;
            if (_iPad.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                _cs.logger.WriteLine("iPad for room:" + _room.GetRoomName() + ", not registered successfully");
            }

            SubscribeToRoomEvents();
        }

        private void _iPad_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            _cs.logger.WriteLine("iPad connected to room: " + _room.GetRoomName() + ", " + args.DeviceOnLine);
            if(args.DeviceOnLine)
            {
                InitializeValues();
                ChangeSubpage(30);
                UpdateSourceSelected(_room.GetSourceSelected());
            }
            else
            {
                aTimer.Stop();
            }
        }

        void InitializeValues()
        {
            _iPad.StringInput[1].StringValue = _room.GetRoomName();
            _iPad.StringInput[2].StringValue = _room.GetRoomVolLevel() + "%";
            _iPad.BooleanInput[22].BoolValue = _room.GetMuteState();
        }

        private void _iPad_SigChange(Crestron.SimplSharpPro.DeviceSupport.BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    if (args.Sig.Number == 10 && args.Sig.BoolValue == true)
                    {
                        ChangeSubpage(31);
                    }
                    if (args.Sig.Number == 20 && args.Sig.BoolValue == true)
                    {
                        _room.VolUp();
                    }
                    if (args.Sig.Number == 21 && args.Sig.BoolValue == true)
                    {
                        _room.VolDown();
                    }
                    if (args.Sig.Number == 22 && args.Sig.BoolValue == true)
                    {
                        _room.Mute();
                    }
                    if (args.Sig.Number == 100 && args.Sig.BoolValue == true)
                    {
                        _room.SetSourceSelected(0);
                    }
                    if (args.Sig.Number == 101 && args.Sig.BoolValue == true)
                    {
                        if (_iPad.BooleanInput[101].BoolValue == true)
                            ChangeSubpage(32);
                        else
                            _room.SetSourceSelected(1);
                    }

                    //IPTV-Main Signals
                    if (args.Sig.Number == 200 && args.Sig.BoolValue == true)
                        ChangeSubpage(31);
                    if (args.Sig.Number == 201 && args.Sig.BoolValue == true)
                        ChangeSubpage(33);
                    if (args.Sig.Number == 202 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(0);
                    if (args.Sig.Number == 203 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(1);
                    if (args.Sig.Number == 204 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(2);
                    if (args.Sig.Number == 205 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(3);
                    if (args.Sig.Number == 206 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(4);
                    if (args.Sig.Number == 207 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(5);
                    if (args.Sig.Number == 208 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(6);
                    if (args.Sig.Number == 209 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(7);
                    if (args.Sig.Number == 210 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(8);

                    //IPTV-Numpad Signals
                    if (args.Sig.Number == 211 && args.Sig.BoolValue == true)
                        ChangeSubpage(32);
                    if (args.Sig.Number == 212 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(9);
                    if (args.Sig.Number == 213 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(10);
                    if (args.Sig.Number == 214 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(11);
                    if (args.Sig.Number == 215 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(12);
                    if (args.Sig.Number == 216 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(13);
                    if (args.Sig.Number == 217 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(14);
                    if (args.Sig.Number == 218 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(15);
                    if (args.Sig.Number == 219 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(16);
                    if (args.Sig.Number == 220 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(17);
                    if (args.Sig.Number == 221 && args.Sig.BoolValue == true)
                        _room.SourceBtnPressed(18);

                    //Hold top Logic
                    if (args.Sig.Number == 300 && args.Sig.BoolValue == true)
                    {
                        if (timerRunning) return;

                        SetTimer();
                        holding = true;
                    }
                    if (args.Sig.Number == 300 && args.Sig.BoolValue == false)
                        holding = false;

                    //Hidden Settings
                    if (args.Sig.Number == 301 && args.Sig.BoolValue == true)
                        ChangeSubpage(31);
                    if (args.Sig.Number == 302 && args.Sig.BoolValue == true)
                        ChangeSubpage(35);
                    if (args.Sig.Number == 303 && args.Sig.BoolValue == true)
                    {
                        _room.SetSourceSelected(-1);
                        ChangeSubpage(31);
                    }

                    //Sky-Main
                    if (args.Sig.Number == 400 && args.Sig.BoolValue == true)
                        ChangeSubpage(34);
                    if (args.Sig.Number == 401 && args.Sig.BoolValue == true)
                        ChangeSubpage(36);
                    if (args.Sig.Number == 402 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(0);
                    if (args.Sig.Number == 403 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(1);
                    if (args.Sig.Number == 404 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(2);
                    if (args.Sig.Number == 405 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(3);
                    if (args.Sig.Number == 406 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(4);
                    if (args.Sig.Number == 407 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(5);
                    if (args.Sig.Number == 408 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(6);
                    if (args.Sig.Number == 409 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(30);
                    if (args.Sig.Number == 410 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(8);

                    //Sky-Numpad
                    if (args.Sig.Number == 411 && args.Sig.BoolValue == true)
                        ChangeSubpage(35);
                    if (args.Sig.Number == 412 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(9);
                    if (args.Sig.Number == 413 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(10);
                    if (args.Sig.Number == 414 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(11);
                    if (args.Sig.Number == 415 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(12);
                    if (args.Sig.Number == 416 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(13);
                    if (args.Sig.Number == 417 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(14);
                    if (args.Sig.Number == 418 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(15);
                    if (args.Sig.Number == 419 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(16);
                    if (args.Sig.Number == 420 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(17);
                    if (args.Sig.Number == 421 && args.Sig.BoolValue == true)
                        _room.SkyBtnPressed(18);
                    break;
            }
        }

        void ChangeSubpage(uint subpageNum)
        {
            //30 - ScreenSaver
            //31 - HomePage
            //32 - IPTV-Main
            //33 - IPTV-Numpad
            //34 - Hidden Settings
            //35 - Sky-Main
            //36 - Sky-Numpad

            for(uint i = 30; i < 40; i++)
                _iPad.BooleanInput[i].BoolValue = false;

            _iPad.BooleanInput[subpageNum].BoolValue = true;
        }

        private void SetTimer()
        {
            aTimer = new Timer(5000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
            timerRunning = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(holding)
                ChangeSubpage(34);

            timerRunning = false;
        }

        void SubscribeToRoomEvents()
        {
            _room.RoomTVConnectedEvent += _room_RoomTVConnectedEvent;
            _room.RoomVolChangedEvent += _room_RoomVolChangedEvent;
            _room.SourceSelectedEvent += _room_SourceSelectedEvent;
            _room.RoomMuteStateChangedEvent += _room_RoomMuteStateChangedEvent;
            _room.RoomsJoinedEvent += _room_RoomsJoinedEvent;
            _room.SlaveModeEvent += _room_SlaveModeEvent;
        }

        private void _room_SlaveModeEvent(bool obj)
        {

        }

        private void _room_RoomsJoinedEvent(bool obj)
        {

        }

        private void _room_RoomMuteStateChangedEvent(bool obj)
        {
            _iPad.BooleanInput[22].BoolValue = obj;
        }

        private void _room_SourceSelectedEvent(short obj)
        {
            UpdateSourceSelected(obj);
        }

        private void _room_RoomVolChangedEvent(int obj)
        {
            _iPad.StringInput[2].StringValue = obj.ToString() + "%";
        }

        private void _room_RoomTVConnectedEvent(bool obj)
        {

        }

        void UpdateSourceSelected(int newSource)
        {
            if (newSource == 0)
            {
                _iPad.BooleanInput[100].BoolValue = true;
                _iPad.BooleanInput[101].BoolValue = false;
            }
            if (newSource == 1)
            {
                _iPad.BooleanInput[101].BoolValue = true;
                _iPad.BooleanInput[100].BoolValue = false;
            }
            if (newSource == -1)
            {
                _iPad.BooleanInput[101].BoolValue = false;
                _iPad.BooleanInput[100].BoolValue = false;
            }
        }
    }
}
