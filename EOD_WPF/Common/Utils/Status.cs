using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public static class Status
    {
        public const string SERVER_DISCONNECTED = "Waiting for client";
        public const string PREPARING = "Getting everything ready";
        public const string READY = "Ready";
        public const string RECORDING = "Recording";
        public const string STOPPED = "Stopped";
        public const string STORING = "Storing session please wait...";

        public const string CLIENT_DISCONNECTED = "Waiting for server IP";
        public const string CONNECTED = "Connected";
        public const string DISCONNECTED = "Disconnected";
    }
}
