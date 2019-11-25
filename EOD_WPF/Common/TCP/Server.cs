using System;
using System.Net;
using System.Net.Sockets;
using Common.Utils;
using Newtonsoft.Json;

namespace Common.TCP
{
    public class Server : IClientServer
    {
        private TcpListener TcpListener;
        private string PartialMessage = "";
        public event EventHandler ServerPropertyChanged
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
        public event EventHandler<NewRemoteFrameArgs> ServerNewRemoteFrameArrived;
        public Server(int port) : base(1000000)
        {
            Status = Utils.Status.SERVER_DISCONNECTED;
            TcpListener = new TcpListener(IPAddress.Any, port);
            ConnectWithClient();
        }
        private void ConnectWithClient()
        {
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(OnClientConnection, null);
        }
        private void OnClientConnection(IAsyncResult ar)
        {
            try
            {
                if(TcpListener != null)
                {
                    TcpClient = TcpListener.EndAcceptTcpClient(ar);
                    connected = true;
                    Status = Utils.Status.CONNECTED;
                    Start();
                }
            }
            catch { }
        }
        private void LostConnection()
        {
            connected = false;
            Status = Utils.Status.SERVER_DISCONNECTED;
            Dispose();
        }
        public void Start()
        {
            try
            {
                NetworkStream.Write(Signals.START);
                ReceiveFrame();
                Status = Utils.Status.PREPARING;
            }
            catch
            {
                LostConnection();
            }
        }
        public void Stop()
        {
            try
            {
                NetworkStream.Write(Signals.STOP);
                Status = Utils.Status.STOPPED;
            }
            catch
            {
                LostConnection();
            }
        }
        public void Prepare()
        {
            try
            {
                NetworkStream.Write(Signals.PREPARE);
                Status = Utils.Status.PREPARING;
            }
            catch
            {
                LostConnection();
            }
        }
        public void ForceFPS(bool force15FPS)
        {
            try
            {
                NetworkStream.Write(force15FPS ? Signals.FORCE15 : Signals.FORCE30);
            }
            catch
            {
                LostConnection();
            }
        }
        public void Close()
        {
            try
            {
                if(NetworkStream != null)
                {
                    NetworkStream.Write(Signals.CLOSE);
                }
                Dispose();
                if (TcpListener != null)
                {
                    TcpListener.Server.Close();
                    TcpListener.Stop();
                    TcpListener = null;
                }
            }
            catch
            {
            }
        }
        private void ReceiveFrame()
        {
            NetworkStream.BeginRead(ReadBuffer, OnFrameReceived);
        }
        private void OnFrameReceived(IAsyncResult ar)
        {
            try
            {
                string message = ReadBuffer.ToString(NetworkStream.EndRead(ar));
                ReceiveFrame();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    DigestMessage(message);
                }
            }
            catch(Exception ex)
            {
                LostConnection();
            }
        }
        object lockObj = new object();
        private void DigestMessage(string message)
        {
            lock (lockObj)
            {
                if (message.Contains("@"))
                {
                    message = PartialMessage + message;
                    while (message.Contains("@"))
                    {
                        int indexofseparator = message.IndexOf("@");
                        ConsumeMessage(message.Substring(0, indexofseparator));
                        message = message.Substring(indexofseparator + 1);
                    }
                    PartialMessage = message;
                }
                else
                {
                    PartialMessage += message;
                }
            }
        }
        private void ConsumeMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    NewRemoteFrameArgs newFrame = JsonConvert.DeserializeObject<NewRemoteFrameArgs>(message);
                    ServerNewRemoteFrameArrived?.Invoke(this, newFrame);
                }
                catch{}
            }
        }
        public override void Dispose()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
            }            
            ReadBuffer = null;
        }
    }
}
