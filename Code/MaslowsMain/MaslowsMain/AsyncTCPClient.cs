using MaslowsMain;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MessageReceivedEventArgs : EventArgs
{
    public byte[] message { get; set; }
}

public class AsyncTCPClient
{
    ControlSystem cs;
    int noData = 0;
    List<int> _connRequests;

    Socket _clientSocket;
    string _IPADDRESS;
    int _PORT;
    byte[] _buffer;
    int _bufferSize;

    public delegate void MessageReceivedEventHandler(object source, MessageReceivedEventArgs args);
    public event MessageReceivedEventHandler MessageReceived;
    public event Action<bool> ConnectedEvent;

    public AsyncTCPClient(ControlSystem controlSys, string IPAddr, int port, int bufferSize)
    {
        cs = controlSys;
        _IPADDRESS = IPAddr;
        _PORT = port;
        _bufferSize = bufferSize;
        _connRequests = new List<int>();
    }

    public void SendMessage(string message)
    {
        try
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            _clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
        }
        catch (SocketException) { }
        catch (Exception ex)
        {
            cs.logger.WriteLine("Exception in AsyncTCPClient.SendMessage() " + ex.ToString());
        }
    }

    public void SendByteMessage(byte[] message)
    {
        try
        {
            byte[] buffer = message;
            _clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
        }
        catch (SocketException) { }
        catch (Exception ex)
        {
            cs.logger.WriteLine("Exception in AsyncTCPClient.SendByteMessage() " + ex.ToString());
        }
    }

    void SendCallback(IAsyncResult AR)
    {
        try
        {
            _clientSocket.EndSend(AR);
        }
        catch (Exception ex)
        {
            cs.logger.WriteLine("Exception in AsyncTCPClient.SendCallback() " + ex.ToString());
        }
    }

    public void ReceiveCallback(IAsyncResult AR)
    {
        try
        {
            int received = _clientSocket.EndReceive(AR);
            Array.Resize(ref _buffer, received);
            string text = Encoding.ASCII.GetString(_buffer);

            if (text.Equals(""))
            {
                noData++;
                if (noData == 10)
                {
                    HandleDisconnect();
                    OnDeviceConnected(false);
                    noData = 0;
                }
            }

            OnMessageReceived(_buffer);

            Array.Resize(ref _buffer, _bufferSize);
            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
        catch (ObjectDisposedException)
        { cs.logger.WriteLine("TCP Client Socket for: " + _IPADDRESS + ":" + _PORT + " Disposed"); }
        catch (Exception)
        {
            cs.logger.WriteLine("Exception in AsyncTCPClient.ReceiveCallback(): No longer connected to " + _IPADDRESS + ":" + _PORT);
            HandleDisconnect();
        }
    }

    static void Delay(int milisecondsDelay)
    {
        Thread.Sleep(milisecondsDelay);
        return;
    }

    public void Connect()
    {
        try
        {
            if (!GetConnectionStatus())
            {
                IPHostEntry connectToAddress = Dns.GetHostEntry(_IPADDRESS);
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.BeginConnect(new IPEndPoint(connectToAddress.AddressList[0], _PORT), new AsyncCallback(ConnectCallback), null);
                _buffer = new byte[_bufferSize];
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
        }
        catch (Exception ex)
        {
            cs.logger.WriteLine("Exception in AsyncTCPClient.Connect() " + ex.ToString());
        }
    }

    void ConnectCallback(IAsyncResult AR)
    {
        try
        {
            OnDeviceConnected(GetConnectionStatus());
            _clientSocket.EndConnect(AR);
        }
        catch (Exception ex)
        {
            cs.logger.WriteLine("Exception in AsyncTCPClient.ConnectCallback() " + ex.ToString());
        }
    }

    public void OnDeviceConnected(bool connStatus)
    {
        if (this.ConnectedEvent != null)
        {
            this.ConnectedEvent(connStatus);
        }
    }

    public void Disconnect(int tpID)
    {
        cs.logger.WriteLine("Trying to dsiconnect " + _IPADDRESS + ":" + _PORT + ", current connRequests: " + _connRequests.Count);
        if (_clientSocket != null)
        {
            if(_connRequests.IndexOf(tpID) != -1)
                _connRequests.Remove(tpID);

            if (_clientSocket.Connected && _connRequests.Count < 1)
                _clientSocket.Dispose();
        }
    }

    public void ConnectRequest(int tpID)
    {
        if(_connRequests.IndexOf(tpID) == -1)
            _connRequests.Add(tpID);

        if (_connRequests.Count == 1)
            Connect();
    }

    void HandleDisconnect()
    {
        _clientSocket.Dispose();
        Delay(500);

        if (_clientSocket.Connected)
        {
            cs.logger.WriteLine("Client Still Connected, Returning...");
            return;
        }

        if(!_clientSocket.Connected && _connRequests.Count > 0)
        {
            cs.logger.WriteLine("Trying to reconnect...");
            Connect();
        }

        if (!_clientSocket.Connected && _connRequests.Count < 1)
        {
            cs.logger.WriteLine("Client Not Connected and no active requests, Returning...");
            return;
        }
    }

    public bool GetConnectionStatus()
    {
        if (_clientSocket != null)
            return _clientSocket.Connected;
        else
            return false;
    }

    protected virtual void OnMessageReceived(byte[] message)
    {
        if (MessageReceived != null)
            MessageReceived(this, new MessageReceivedEventArgs() { message = message });
    }
}

