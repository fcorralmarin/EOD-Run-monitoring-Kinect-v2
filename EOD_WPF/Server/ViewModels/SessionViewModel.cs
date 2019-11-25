using Common.KinectUtils;
using Common.Utils;
using Common.Utils.Core;
using Common.Utils.Core.Commands;
using Newtonsoft.Json;
using Server.DB;
using Server.Models;
using Server.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Server.ViewModels
{
    public class SessionViewModel : IBase
    {
        private bool recording = false;
        private DispatcherTimer EndOfSessionTimer;
        private DispatcherTimer SessionPercentageTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
        private SaveSession SaveSessionWindow;
        
        private int MinimumDuration = 30;
        
        private Queue<NewRemoteFrameArgs> LocalFrames = new Queue<NewRemoteFrameArgs>();
        private Queue<NewRemoteFrameArgs> RemoteFrames = new Queue<NewRemoteFrameArgs>();

        public SessionViewModel()
        {
            RemoteKinect = new RemoteKinect();
            RemoteKinect.Server = new Common.TCP.Server(Properties.Settings.Default.Port);
            RemoteKinect.Server.ServerPropertyChanged += new EventHandler((sender, e) => { RaisePropertyChanged("RemoteKinect"); RaisePropertyChanged("CanCancel"); RaisePropertyChanged("CanPrepare"); RaisePropertyChanged("CanForceFPS"); });
            RemoteKinect.Server.ServerNewRemoteFrameArrived += Server_ServerNewRemoteFrameArrived;

            LocalKinect = new LocalKinect();
            LocalKinect.KinectController.PropertyChanged += new EventHandler((sender, e) => { RaisePropertyChanged("LocalKinect"); RaisePropertyChanged("CanCancel"); });
            LocalKinect.KinectController.NewLocalFrameArrived += KinectController_NewLocalFrameArrived;
            Left = new Graphics();
            Right = new Graphics();

            EndOfSessionTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, MaximumDuration) };
            EndOfSessionTimer.Tick += EndOfSessionTimerEvent;
            SessionPercentageTimer.Tick += SessionPercentageTimer_Tick;
        }

        #region Properties
        public ObservableCollection<string> IP
        {
            get
            {
                return new ObservableCollection<string>(Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.AddressFamily == AddressFamily.InterNetwork).Select(x => x.ToString()));
            }
        }
        
        public int MaximumDuration { get; } = 60;

        private int currentDuration = 0;
        public int CurrentDuration
        {
            get { return currentDuration; }
            set
            {
                currentDuration = value;
                RaisePropertyChanged("CurrentDuration");
                RaisePropertyChanged("SessionPercentageColor");
                RaisePropertyChanged("SessionPercentage");
                RaisePropertyChanged("CanStop");
            }
        }

        public int SessionPercentage
        {
            get
            {
                return CurrentDuration * 100 / MaximumDuration;
            }
        }

        public string SessionPercentageColor
        {
            get
            {
                return CurrentDuration >= MinimumDuration ? "LightGreen" : "IndianRed";
            }
        }

        private LocalKinect localKinect;
        public LocalKinect LocalKinect
        {
            get
            {
                return localKinect;
            }
            set
            {
                localKinect = value;
                RaisePropertyChanged("LocalKinect");
            }
        }

        private RemoteKinect remoteKinect;
        public RemoteKinect RemoteKinect
        {
            get
            {
                return remoteKinect;
            }
            set
            {
                remoteKinect = value;
                RaisePropertyChanged("RemoteKinect");
            }
        }

        private Graphics left;
        public Graphics Left
        {
            get { return left; }
            set
            {
                left = value;
                RaisePropertyChanged("Left");
            }
        }

        private Graphics right;
        public Graphics Right
        {
            get { return right; }
            set
            {
                right = value;
                RaisePropertyChanged("Right");
            }
        }

        private SessionModel session;
        public SessionModel Session
        {
            get { return session; }
            set
            {
                session = value;
                RaisePropertyChanged("Session");
            }
        }
        #endregion

        #region Init
        private ICommand initializeCommand;
        public ICommand InitializeCommand
        {
            get
            {
                if (initializeCommand == null)
                    initializeCommand = new RelayCommand(new Action(Initialize));
                return initializeCommand;
            }
        }

        private void Initialize()
        {
            LocalKinect.KinectController.InitKinect();
            LocalKinect.KinectController.Start();
        }
        #endregion

        #region Preparation
        public bool CanPrepare
        {
            get
            {
                return RemoteKinect.Server.Connected && !RemoteKinect.Prepared && !LocalKinect.Prepared && LocalKinect.KinectController.Status == Status.CONNECTED;
            }
        }

        private ICommand prepareCommand;
        public ICommand PrepareCommand
        {
            get
            {
                if (prepareCommand == null)
                    prepareCommand = new RelayCommand(new Action(Prepare));
                return prepareCommand;
            }
        }

        private void Prepare()
        {
            MessageBox.Show("Please, raise your arms, cameras are going to infer its position using your arms position.");
            RemoteKinect.Server.Prepare();
            LocalKinect.KinectController.Prepare();
        }
        #endregion

        #region Force FPS
        public bool CanForceFPS
        {
            get
            {
                return CanPrepare || CanRecord;
            }
        }

        private ICommand forceFPSCommand;
        public ICommand ForceFPSCommand
        {
            get
            {
                if (forceFPSCommand == null)
                    forceFPSCommand = new ParamCommand(new Action<object>(ForceFPS));
                return forceFPSCommand;
            }
        }

        private void ForceFPS(object obj)
        {
            LocalKinect.KinectController.ForceFPS((bool)obj);
            RemoteKinect.Server.ForceFPS((bool)obj);
        }
        #endregion

        #region Start Recording

        private void SessionPercentageTimer_Tick(object sender, EventArgs e)
        {
            CurrentDuration = Math.Min(MaximumDuration, CurrentDuration + 1);            
        }

        public bool CanRecord
        {
            get
            {
                return RemoteKinect.Server.Connected && RemoteKinect.Prepared && LocalKinect.Prepared && LocalKinect.KinectController.Status == Status.CONNECTED && !recording;
            }
        }

        private ICommand recordCommand;
        public ICommand RecordCommand
        {
            get
            {
                if (recordCommand == null)
                    recordCommand = new RelayCommand(new Action(Record));
                return recordCommand;
            }
        }

        private void Record()
        {
            RemoteKinect.Server.Status = Status.RECORDING;
            recording = true;
            RaisePropertyChanged("CanRecord");
            RaisePropertyChanged("CanForceFPS");
            CurrentDuration = 1;
            Right = new Graphics();
            Left = new Graphics();
            Task.Factory.StartNew(() =>
           {
               EndOfSessionTimer.Start();
               SessionPercentageTimer.Start();
               Session = DBUpdater.CreateUnsavedSession();
           });
        }
        #endregion

        #region Stop recording
        public bool CanStop
        {
            get
            {
                return RemoteKinect.Server.Status == Status.RECORDING && CurrentDuration >= MinimumDuration;
            }
        }

        private ICommand stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (stopCommand == null)
                    stopCommand = new RelayCommand(new Action(Stop));
                return stopCommand;
            }
        }

        private void Stop()
        {
            CommonEndOfSession();
        }

        #endregion

        #region Save Recording
        private void EndOfSessionTimerEvent(object sender, EventArgs e)
        {
            CurrentDuration = MaximumDuration;
            CommonEndOfSession();
        }

        private void CommonEndOfSession()
        {
            recording = false;
            LocalKinect.KinectController.Stop();
            RemoteKinect.Server.Stop();
            SessionPercentageTimer.Stop();
            EndOfSessionTimer.Stop();
            Session.Duration = CurrentDuration;
            SaveSessionWindow = new SaveSession(Session);
            SaveSessionWindow.WindowClosed += SaveSessionWindow_WindowClosed;
            SaveSessionWindow.Show();
            RaisePropertyChanged("CanRecord");
            RaisePropertyChanged("CanForceFPS");
        }

        private void SaveSessionWindow_WindowClosed(object sender, bool saveSession)
        {
            SaveSessionWindow.Close();
            if (saveSession)
            {
                RemoteKinect.Server.Status = Status.STORING;
                DBUpdater.DigestSession(Session, ref LocalFrames, ref RemoteFrames);
            }
            else
            {
                LocalFrames.Clear();
                RemoteFrames.Clear();
            }
            CurrentDuration = 0;
            LocalKinect.KinectController.Start();
            RemoteKinect.Server.Start();
            RemoteKinect.Server.Status = Status.READY;
        }
        #endregion

        #region Cancel Recording
        public bool CanCancel
        {
            get
            {
                return RemoteKinect.Server.Status == Status.RECORDING;
            }
        }

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                    cancelCommand = new RelayCommand(new Action(Cancel));
                return cancelCommand;
            }
        }

        private void Cancel()
        {
            RemoteKinect.Server.Status = Status.READY;
            recording = false;
            SessionPercentageTimer.Stop();
            EndOfSessionTimer.Stop();
            LocalFrames.Clear();
            RemoteFrames.Clear();
            CurrentDuration = 0;
            RaisePropertyChanged("CanRecord");
            RaisePropertyChanged("CanForceFPS");
        }

        #endregion

        #region Dispose
        private ICommand disposeCommand;
        public ICommand DisposeCommand
        {
            get
            {
                if (disposeCommand == null)
                    disposeCommand = new RelayCommand(new Action(Dispose));
                return disposeCommand;
            }
        }

        private void Dispose()
        {
            Cancel();
            LocalKinect.KinectController.Close();
            RemoteKinect.Server.Close();
        }
        #endregion

        #region Graphics
        private void OnBodyReceived(Gait gait)
        {
            switch (gait.POV)
            {
                case POV.Left:
                    Left.Add(gait);
                    Left.Refresh();
                    RaisePropertyChanged("Left");
                    break;
                case POV.Right:
                    Right.Add(gait);
                    Right.Refresh();
                    RaisePropertyChanged("Right");
                    break;
            }
        }

        #endregion

        object lockloc = new object();
        private void KinectController_NewLocalFrameArrived(object sender, NewLocalFrameArgs localEvent)
        {
            lock (lockloc)
            {
                if (localEvent.Gait.POV != POV.Unknown)
                {
                    if (!LocalKinect.Prepared)
                    {
                        LocalKinect.Prepared = true;
                        RaisePropertyChanged("CanRecord");
                        RaisePropertyChanged("CanPrepare");
                        RaisePropertyChanged("CanForceFPS");
                    }
                    OnBodyReceived(localEvent.Gait);
                }
                if(localEvent.Frame != null)
                {
                    LocalKinect.Image = localEvent.Frame;
                    RaisePropertyChanged("LocalKinect");
                }
                if (recording)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        LocalFrames.Enqueue(new NewRemoteFrameArgs()
                        {
                            FPS = localEvent.FPS,
                            Frame = localEvent.Frame.ToBase64(),
                            Gait = new Gait(localEvent.Gait),
                            TimeStamp = localEvent.TimeStamp
                        });
                    }));
                    //NewRemoteFrameArgs s1 = new NewRemoteFrameArgs
                    //{
                    //    FPS = localEvent.FPS,
                    //    Frame = localEvent.Frame.ToBase64(),
                    //    Gait = localEvent.Gait,
                    //    TimeStamp = localEvent.TimeStamp
                    //};
                    //LocalFrames.Enqueue(s1);
                }
            }
        }

        object lockerem = new object();
        private void Server_ServerNewRemoteFrameArrived(object sender, NewRemoteFrameArgs remoteEvent)
        {
            lock (lockerem)
            {
                if (remoteEvent.Gait.POV != POV.Unknown)
                {
                    if (!RemoteKinect.Prepared)
                    {
                        RemoteKinect.Prepared = true;
                        RaisePropertyChanged("CanRecord");
                        RaisePropertyChanged("CanPrepare");
                        RaisePropertyChanged("CanForceFPS");
                    }
                    OnBodyReceived(remoteEvent.Gait);
                }
                if(remoteEvent.Frame != null)
                {
                    RemoteKinect.Image = remoteEvent.Frame.ToBitmapSource();
                    RaisePropertyChanged("RemoteKinect");
                }
                if (recording)
                {
                    RemoteFrames.Enqueue(remoteEvent);
                }
            }
        }
    }
}
