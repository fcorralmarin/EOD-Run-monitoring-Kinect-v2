using Common.KinectUtils;
using Common.Utils;
using Common.Utils.Core;
using Common.Utils.Core.Commands;
using Server.DB;
using Server.Models;
using Server.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Server.ViewModels
{
    public class PlaySessionViewModel : IBase
    {
        private PlaySession Window;
        private DispatcherTimer Timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 28) };
        private int Index = 0;
        private Dictionary<string, List<string>> SessionFrames;
        private Dictionary<string, List<Gait>> SessionGaits;

        private int IndexTotal = 1;
        public int IndexPercentage
        {
            get
            {
                return Index * 100 / IndexTotal;
            }
        }

        private BitmapSource localImage;
        public BitmapSource LocalImage
        {
            get { return localImage; }
            set
            {
                localImage = value;
                RaisePropertyChanged("LocalImage");
            }
        }

        private BitmapSource remoteImage;
        public BitmapSource RemoteImage
        {
            get { return remoteImage; }
            set
            {
                remoteImage = value;
                RaisePropertyChanged("RemoteImage");
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

        public PlaySessionViewModel(PlaySession window, Models.SessionModel session)
        {
            SessionGaits = new Dictionary<string, List<Gait>>();
            Right = new Graphics();
            Left = new Graphics();
            this.session = session;
            Window = window;
            Timer.Tick += Timer_Tick;  
            
            SessionFrames = System.IO.File.ReadAllBytes(session.GetPath()).Unzip()
                .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(x => x.Split('@')[0])
                .ToDictionary
                (
                    x => x.Key == "1" ? "LOCAL" : "REMOTE",
                    y => y.Select(x => x.Split('@')[1]).ToList()
                );
            IndexTotal = Math.Min(SessionFrames["LOCAL"].Count, SessionFrames["REMOTE"].Count);
            SessionGaits["LOCAL"] = DBAccessor.GetGaits(session.GetHash(), true);
            SessionGaits["REMOTE"] = DBAccessor.GetGaits(session.GetHash(), false);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(Math.Min(SessionFrames["LOCAL"].Count, SessionFrames["REMOTE"].Count) > Index)
            {
                try
                {
                    RemoteImage = SessionFrames["REMOTE"][Index].ToBitmapSource();
                }
                catch { }
                try
                {
                    LocalImage = SessionFrames["LOCAL"][Index].ToBitmapSource();
                }
                catch { }

                OnGaitReceived(SessionGaits["REMOTE"][Index]);
                OnGaitReceived(SessionGaits["LOCAL"][Index]);
                Index++;
                RaisePropertyChanged("IndexPercentage");
            }
            else
            {
                Index = 0;
                Timer.Stop();
            }
        }

        private Models.SessionModel session = new Models.SessionModel();
        public Models.SessionModel Session
        {
            get { return session; }
            set { session = value; }
        }

        private ICommand playCommand;
        public ICommand PlayCommand
        {
            get
            {
                if (playCommand == null)
                    playCommand = new RelayCommand(new Action(Play));
                return playCommand;
            }
        }

        private void Play()
        {
            Timer.Start();
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand
        {
            get
            {
                if (pauseCommand == null)
                    pauseCommand = new RelayCommand(new Action(Pause));
                return pauseCommand;
            }
        }

        private void Pause()
        {
            Timer.Stop();
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
            Timer.Stop();
            Index = 0;
            try
            {
                RemoteImage = SessionFrames["REMOTE"][Index].ToBitmapSource();
            }
            catch { }
            try
            {
                LocalImage = SessionFrames["LOCAL"][Index].ToBitmapSource();
            }
            catch { }
            Left.Clear();
            Left.Refresh();
            Right.Clear();
            Right.Refresh();
            RaisePropertyChanged("IndexPercentage");
        }


        private void OnGaitReceived(Gait gait)
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
    }
}
