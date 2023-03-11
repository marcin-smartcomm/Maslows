using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using WebsocketServer;

namespace MaslowsMain
{
    public class ConsoleLogger
    {
        ControlSystem _cs;

        private WebsocketSrvr _server;
        private bool _clientConnected;

        private List<string> _backlog;

        public ConsoleLogger(int port, ControlSystem cs)
        {
            _cs = cs;

            try
            {
                _server = new WebsocketSrvr();
                _server.Initialize(port);
                _server.OnClientConnectedChange += OnClientConnected;
                _server.OnStringSignalChange += OnReceivingMessage;

                _backlog = new List<string>();

                _clientConnected = false;
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
            }
        }

        public void Start()
        {
            _server.StartServer();
        }

        public void Stop()
        {
            _server.StopServer();
        }

        public void WriteLine(string msg, params object[] args)
        {
            var text = String.Format(msg, args) + "\n";

            if (_clientConnected)
            {
                _server.SetIndirectTextSignal(1, text);
            }
            else
            {
                _backlog.Add(text);
                if(_backlog.Count == 101)
                    _backlog.Clear();
            }
        }

        private void OnClientConnected(ushort state)
        {
            if (state == 0)
            {
                // Disconnected
                _clientConnected = false;
            }
            else
            {
                // Connected
                _clientConnected = true;
                _server.SetIndirectTextSignal(1, "\n-- CONNECTED --\n");

                if (_backlog.Count > 0)
                {
                    foreach (var msg in _backlog)
                    {
                        _server.SetIndirectTextSignal(1, msg);
                    }
                }

                _backlog.Clear();
            }
        }


        private void OnReceivingMessage(ushort state, SimplSharpString value)
        {
            if (value.ToString() == "__ping__")
            {
                _server.SetIndirectTextSignal(1, "__pong__");
            }
            if (value.ToString().Contains("FireAlarm"))
            {
                if (value.ToString().Split(':')[1].Equals("true"))
                    _cs.FireAlarmState(true);
                else if (value.ToString().Split(':')[1].Equals("false"))
                    _cs.FireAlarmState(false);
                else
                    _server.SetIndirectTextSignal(1, "Invalid Command");
            }
            if (value.ToString().Contains("Relays"))
            {
                if (value.ToString().Split(':')[1].Equals("Open"))
                    _cs.OpenRelays();
                if (value.ToString().Split(':')[1].Equals("Close"))
                    _cs.CloseRelays();
            }
        }
    }
}
