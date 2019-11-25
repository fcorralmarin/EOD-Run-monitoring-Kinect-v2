using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.TCP
{
    public abstract class IClientServer : IDisposable
    {
        protected event EventHandler PropertyChanged;

        protected TcpClient TcpClient;
        protected byte[] ReadBuffer;

        protected bool connected;
        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                if (connected != value)
                {
                    connected = value;
                    PropertyChanged?.Invoke(this, null);
                }
            }
        }

        protected string status;

        protected IClientServer(int bufferSize)
        {
            TcpClient = new TcpClient();
            ReadBuffer = new byte[bufferSize];
            Connected = false;
        }

        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    status = value;
                    PropertyChanged?.Invoke(this, null);
                }
            }
        }

        protected NetworkStream NetworkStream
        {
            get
            {
                if (TcpClient != null && TcpClient.Connected)
                {
                    return TcpClient.GetStream();
                }
                else
                {
                    return null;
                }
            }
        }

        public abstract void Dispose();
    }
}
