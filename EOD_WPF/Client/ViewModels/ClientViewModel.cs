using Common.KinectUtils;
using Common.Utils;
using Common.Utils.Core;
using Common.Utils.Core.Commands;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Client.ViewModels
{
    public class ClientViewModel : IBase
    {
        private Views.Client Window;
        private DispatcherTimer CloseTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };

        public ClientViewModel(Views.Client window)
        {
            Window = window;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.IP))
            {
                IP = Properties.Settings.Default.IP;
            }
            else
            {
                IP = "";
            }

            KinectController = new KinectController(false);
            KinectController.PropertyChanged        += new EventHandler((sender, e) => { RaisePropertyChanged("KinectController"); });
            KinectController.NewRemoteFrameArrived  += new EventHandler<NewRemoteFrameArgs>((sender, e) => { client.SendFrameToServer(e); });
            KinectController.InitKinect();

            Client = new Common.TCP.Client(Properties.Settings.Default.Port);
            Client.ClientPropertyChanged  += new EventHandler((sender, e) => { RaisePropertyChanged("Client"); });
            Client.ClientStartRequested += new EventHandler((sender, e) => { KinectController.Start();});
            Client.ClientStopRequested  += new EventHandler((sender, e) => { KinectController.Stop(); });
            Client.ClientPrepareRequested  += new EventHandler((sender, e) => { KinectController.Prepare(); });
            client.ClientForceFPSRequested += new EventHandler<bool>((sender, e) => { KinectController.ForceFPS(e); });
            Client.ClientCloseRequested  += new EventHandler((sender, e) => { CloseTimer.Start(); });

            CloseTimer.Tick += CloseTimer_Tick;
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            DisposeAll();
        }

        private KinectController kinectController;
        public KinectController KinectController
        {
            get { return kinectController; }
            set
            {
                kinectController = value;
                RaisePropertyChanged("KinectController");
            }
        }

        private Common.TCP.Client client;
        public Common.TCP.Client Client
        {
            get { return client; }
            set
            {
                client = value;
                RaisePropertyChanged("Client");
            }
        }

        private string ip;
        public string IP
        {
            get{ return ip; }
            set
            {
                ip = value;
                RaisePropertyChanged("IP");
                RaisePropertyChanged("ValidIP");
            }
        }

        private bool ValidIP
        {
            get
            {
                bool isValid = Regex.IsMatch(IP, @"^((\d|\d\d|1\d\d|2[0-4]\d|25[0-5])\.){3}((\d|\d\d|1\d\d|2[0-4]\d|25[0-5]))$");
                return isValid;
            }
        }

        private ICommand connect;

        public ICommand Connect
        {
            get
            {
                if (connect == null)
                    connect = new RelayCommand(new Action(ConnectWithServer), () => ValidIP && !client.Connected);
                return connect;
            }
        }

        private ICommand dispose;
        public ICommand Dispose
        {
            get
            {
                if (dispose == null)
                    dispose = new RelayCommand(new Action(DisposeAll));
                return dispose;
            }
        }

        private void DisposeAll()
        {
            KinectController.Close();
            Client.Dispose();
            Window.CloseWindow();
        }

        private void ConnectWithServer()
        {
            client.ConnectWithServer(IP);
            if (client.Connected)
            {
                Properties.Settings.Default.IP = IP;
                Properties.Settings.Default.Save();
            }
        }
    }
}
