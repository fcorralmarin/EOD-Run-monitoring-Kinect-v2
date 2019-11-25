using Microsoft.Kinect;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;

namespace Common.KinectUtils
{
    public class KinectController
    {
        private bool IsLocal;
        private bool Force15FPS = true;
        private bool IgnoreFrame = false;
        private KinectSensor KinectSensor;
        private MultiSourceFrameReader MultiSourceFrameReader;
        private Int32Rect ROI;

        private List<Skeleton> ReferenceSkeletonSequence = new List<Skeleton>();
        private int? PositionHelper = null;
        private Gait Gait;

        private int fps;
        public int FPS
        {
            get
            {
                return fps;
            }
            set
            {
                if (value != fps)
                {
                    fps = value;
                    PropertyChanged?.Invoke(this, null);
                }
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                if (value != status)
                {
                    status = value;
                    PropertyChanged?.Invoke(this, null);
                }
            }
        }

        public event EventHandler PropertyChanged;
        public event EventHandler<NewLocalFrameArgs> NewLocalFrameArrived;
        public event EventHandler<NewRemoteFrameArgs> NewRemoteFrameArrived;

        public KinectController(bool isLocal)
        {
            FPS = 0;
            IsLocal = isLocal;
            Gait = new Gait();
        }
        private void SetROI(FrameDescription frameDescription)
        {
            ROI = new Int32Rect(frameDescription.Width / 3,
                                            frameDescription.Height / 8,
                                            frameDescription.Width / 3,
                                            frameDescription.Height / 8 * 6);
        }

        public void InitKinect()
        {
            if (KinectSensor == null)
            {
                KinectSensor = KinectSensor.GetDefault();
                KinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;
                KinectSensor.Open();
            }
        }
        public void Start()
        {
            if (!KinectSensor.IsOpen)
            {
                KinectSensor.Open();
            }
            MultiSourceFrameReader = KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            MultiSourceFrameReader.MultiSourceFrameArrived += MultiSourceFrameReader_MultiSourceFrameArrived;
        }
        public void Stop()
        {
            if (KinectSensor != null && KinectSensor.IsOpen)
            {
                KinectSensor.Close();
                MultiSourceFrameReader.Dispose();
                MultiSourceFrameReader = null;
            }
        }
        public void Close()
        {
            if (MultiSourceFrameReader != null)
            {
                MultiSourceFrameReader.Dispose();
                MultiSourceFrameReader = null;
            }
            if (KinectSensor != null)
            {
                KinectSensor.Close();
                KinectSensor = null;
            }
        }
        public void Prepare()
        {
            PositionHelper = 0;
        }
        public void ForceFPS(bool force15FPS)
        {
            Force15FPS = force15FPS;
        }

        private void MultiSourceFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            ColorFrame colorFrame = null;
            BodyFrame bodyFrame = null;
            try
            {
                MultiSourceFrame parentFrame = e.FrameReference.AcquireFrame();
                colorFrame = parentFrame.ColorFrameReference.AcquireFrame();
                bodyFrame = parentFrame.BodyFrameReference.AcquireFrame();

                if (colorFrame != null && bodyFrame != null)
                {
                    FPS = (int)(1.0 / colorFrame.ColorCameraSettings.FrameInterval.TotalSeconds);
                    IgnoreFrame = FPS > 16 && Force15FPS ? !IgnoreFrame : false;

                    if (ROI == null)
                    {
                        SetROI(colorFrame.FrameDescription);
                    }

                    if (bodyFrame != null && PositionHelper.HasValue)
                    {
                        Skeleton skeleton = bodyFrame.ToSkeleton();
                        if (skeleton != null)
                        {
                            if(Gait.POV == POV.Unknown)
                            {
                                if (Math.Abs(PositionHelper.Value) < FPS * 4)
                                {
                                    ReferenceSkeletonSequence.Add(skeleton);
                                    PositionHelper += (skeleton.Joints[JointType.HandLeft].DistanceFromCamera() >
                                                      skeleton.Joints[JointType.HandRight].DistanceFromCamera()) ? 1 : -1;
                                }
                                else
                                {
                                    POV inferredPosition = PositionHelper < 0 ? POV.Left : POV.Right;
                                    Gait = new Gait(ReferenceSkeletonSequence.AvgSkeleton(), inferredPosition);
                                }
                            }
                            else
                            {
                                Gait.Refresh(skeleton);
                            }                            
                        }
                    }

                    BitmapSource newBitmap = null;
                    if (!IgnoreFrame)
                    {
                        newBitmap = colorFrame.ToCroppedBitmap(ROI);
                    }
                    OnFrameProcessed(newBitmap, DateTime.UtcNow.Ticks, FPS);
                }
            }
            finally
            {
                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }
                if (bodyFrame != null)
                {
                    bodyFrame.Dispose();
                }
            }
        }

        private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            Status = e.IsAvailable ? Utils.Status.CONNECTED : Utils.Status.DISCONNECTED;
        }

        object lockObj = new object();

        private void OnFrameProcessed(BitmapSource frame, long timeStamp, int fps)
        {
            lock (lockObj)
            {
                if (IsLocal)
                {
                    NewLocalFrameArrived?.Invoke(this, new NewLocalFrameArgs
                    {
                        FPS = fps,
                        Frame = frame,
                        TimeStamp = timeStamp,
                        Gait = Gait
                    });
                }
                else
                {
                    NewRemoteFrameArrived?.Invoke(this, new NewRemoteFrameArgs
                    {
                        FPS = fps,
                        Frame = frame != null ? frame.ToBase64() : null,
                        TimeStamp = timeStamp,
                        Gait = Gait
                    });
                }
            }
        }
    }
}
