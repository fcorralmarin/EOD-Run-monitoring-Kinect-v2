using Newtonsoft.Json;
using Common.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace Common.TCP
{
    public class Client : IClientServer
    {
        private int ServerPort;

        public event EventHandler ClientPropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }
        }
        public event EventHandler ClientStartRequested;
        public event EventHandler ClientPrepareRequested;
        public event EventHandler<bool> ClientForceFPSRequested;
        public event EventHandler ClientStopRequested;
        public event EventHandler ClientCloseRequested;
        
        private void LostConnection()
        {
            Status = Utils.Status.CLIENT_DISCONNECTED;
            Dispose();
        }
        private void WaitForSignal()
        {
            try
            {
                NetworkStream.BeginRead(ReadBuffer, OnSignalReceived);
            }
            catch
            {
                LostConnection();
            }
        }
        private void OnSignalReceived(IAsyncResult ar)
        {
            try
            {
                string signal = ReadBuffer.ToString(NetworkStream.EndRead(ar));
                switch (signal)
                {
                    case Signals.START:
                        ClientStartRequested?.Invoke(this, null);
                        Status = Utils.Status.CONNECTED;
                        break;
                    case Signals.PREPARE:
                        ClientPrepareRequested?.Invoke(this, null);
                        Status = Utils.Status.CONNECTED;
                        break;
                    case Signals.FORCE15:
                    case Signals.FORCE30:
                        ClientForceFPSRequested?.Invoke(this, signal == Signals.FORCE15);
                        break;
                    case Signals.STOP:
                        ClientStopRequested?.Invoke(this, null);
                        Status = Utils.Status.CONNECTED;
                        break;
                    case Signals.CLOSE:
                        ClientCloseRequested?.Invoke(this, null);
                        Status = "";
                        break;
                }
                if(signal != Signals.CLOSE)
                    WaitForSignal();
            }
            catch
            {
                LostConnection();
            }
        }        
        public Client(int port) : base(4)
        {
            ServerPort = port;
        }
        public void ConnectWithServer(string ip)
        {
            if (!TcpClient.Connected)
            {
                try
                {
                    TcpClient = new TcpClient();
                    TcpClient.Connect(IPAddress.Parse(ip), ServerPort);
                    Status = Utils.Status.CONNECTED;
                    Connected = true;
                    WaitForSignal();
                }
                catch (Exception ex)
                {
                    Status = Utils.Status.CLIENT_DISCONNECTED;
                }
            }
        }
        public void SendFrameToServer(NewRemoteFrameArgs messageArgs)
        {
            string message = null;
            try
            {
                message = JsonConvert.SerializeObject(messageArgs) + "@";
            }
            catch { }
            try
            {
                if(message != null)
                {
                    NetworkStream.BeginWrite(message, (IAsyncResult ar) => 
                    {
                        NetworkStream.EndWrite(ar);
                    });
                }
            }
            catch
            {
                LostConnection();
            }
        }
        public override void Dispose()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
            }
            Connected = false;
        }
    }
}
