using Microsoft.Kinect;
using Common.KinectUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Drawing;
using Size = System.Windows.Size;
using Pen = System.Windows.Media.Pen;
using System.Windows.Media.Media3D;
using LightBuzz.Vitruvius;
using OxyPlot;
using System.IO.Compression;

namespace Common.Utils
{
    public static class Extensions
    {
        #region TCP

        public static byte[] ToBytes(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static string ToString(this byte[] byteArray, int length)
        {
            return Encoding.ASCII.GetString(byteArray, 0, length);
        }

        public static void Write(this NetworkStream ns, string message)
        {
            ns.Write(message.ToBytes(), 0, message.Length);
        }

        public static void BeginRead(this NetworkStream ns, byte[] buffer, AsyncCallback onRead)
        {
            ns.BeginRead(buffer, 0, buffer.Length, onRead, null);
        }

        public static void BeginWrite(this NetworkStream ns, string message, AsyncCallback onWrite)
        {
            ns.BeginWrite(message.ToBytes(), 0, message.Length, onWrite, null);
        }

        #endregion

        #region Camera
        public static byte[] ToByteArray(this BitmapSource bitmapSource)
        {
            byte[] byteArray = new byte[] { };
            if(bitmapSource != null)
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    byteArray = stream.ToArray();
                }
            }
            return byteArray;
        }

        public static string ToBase64(this BitmapSource bitmapSource)
        {
            return Convert.ToBase64String(bitmapSource.ToByteArray());
        }

        public static BitmapSource ToBitmapSource(this string b64)
        {
            var bytes = Convert.FromBase64String(b64);

            using (var stream = new MemoryStream(bytes))
            {
                return BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }
        }

        private static WriteableBitmap ToWriteableBitmap(this ColorFrame colorFrame)
        {
            WriteableBitmap KinectImage = null;
            KinectImage = new WriteableBitmap(colorFrame.FrameDescription.Width, colorFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Gray16, null);
            KinectImage.Lock();
            using (KinectBuffer kinectBuffer = colorFrame.LockRawImageBuffer())
            {
                colorFrame.CopyConvertedFrameDataToIntPtr(
                    KinectImage.BackBuffer,
                    (uint)(colorFrame.FrameDescription.Width * colorFrame.FrameDescription.Height * 2),
                    ColorImageFormat.Yuv);
                KinectImage.AddDirtyRect(new Int32Rect(0, 0, KinectImage.PixelWidth, KinectImage.PixelHeight));
            }
            KinectImage.Unlock();
            return KinectImage;
        }

        public static BitmapSource ToCroppedBitmap(this ColorFrame colorFrame, Int32Rect roi)
        {
            return new CroppedBitmap(colorFrame.ToWriteableBitmap(), roi);
        }
        #endregion

        #region Skeleton
        public static Skeleton ToSkeleton(this BodyFrame bodyFrame)
        {
            Body[] bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);
            Skeleton skeleton = null;
            if (bodies != null && bodies.Length > 0)
            {
                skeleton = new Skeleton(bodies.FirstOrDefault(x => x.IsTracked));
            }
            return skeleton;
        }

        public static Gait AvgGait(this Gait g1, Gait g2)
        {
            return new Gait()
            {
                CurrentAnkleFlexion = (g1.CurrentAnkleFlexion + g2.CurrentAnkleFlexion) / 2,
                CurrentHipFlexion = (g1.CurrentHipFlexion + g2.CurrentHipFlexion) / 2,
                CurrentKneeFlexion = (g1.CurrentKneeFlexion + g2.CurrentKneeFlexion) / 2,
                POV = g1.POV,
                RelaxedAnkleFlexion = (g1.RelaxedAnkleFlexion + g2.RelaxedAnkleFlexion) / 2
            };
        }
        #endregion

        #region Graphics

        public static Skeleton AvgSkeleton(this List<Skeleton> skeletons)
        {
            Skeleton avgSkeleton = new Skeleton();
            avgSkeleton.Joints = new Dictionary<JointType, Joint>();
            foreach (var key in skeletons.First().Joints.Keys)
            {
                Joint newJoint = new Joint()
                {
                    JointType = key,
                    Position = skeletons.Select(x => x.Joints[key].Position).AvgPoints(),
                    TrackingState = TrackingState.Tracked
                };
                avgSkeleton.Joints.Add(key, newJoint);
            }
            return avgSkeleton;
        }

        private static CameraSpacePoint AvgPoints(this IEnumerable<CameraSpacePoint> points)
        {
            return new CameraSpacePoint()
            {
                X = points.Sum(x => x.X) / points.Count(),
                Y = points.Sum(x => x.Y) / points.Count(),
                Z = points.Sum(x => x.Z) / points.Count(),
            };
        }

        public static Vector3D ToVector(this CameraSpacePoint cameraSpacePoint)
        {
            return new Vector3D(cameraSpacePoint.X, cameraSpacePoint.Y, cameraSpacePoint.Z);
        }
        public static CameraSpacePoint ToCamera(this Vector3D vector)
        {
            return new CameraSpacePoint()
            {
                X = (float)vector.X,
                Y = (float)vector.Y,
                Z = (float)vector.Z
            };
        }

        public static double DistanceFromCamera(this Joint joint)
        {
            return joint.Position.JointDistance();
        }

        private static double JointDistance(this CameraSpacePoint from)
        {
            CameraSpacePoint defaultPoint = new CameraSpacePoint()
            {
                X = 0,
                Y = 0,
                Z = 0,
            };
            return from.CameraPointDistance(defaultPoint);
        }

        private static double CameraPointDistance(this CameraSpacePoint from, CameraSpacePoint to)
        {
            CameraSpacePoint sum = new CameraSpacePoint()
            {
                X = from.X - to.X,
                Y = from.Y - to.Y,
                Z = from.Z - to.Z,
            };
            return Math.Sqrt(Math.Pow(sum.X, 2) + Math.Pow(sum.Y, 2) + Math.Pow(sum.Z, 2));
        }

        public static Vector3D BoneVector3(this Joint start, Joint end)
        {
            return Vector3D.Subtract(end.Position.ToVector3(), start.Position.ToVector3());
        }

        #endregion

        #region File Saving
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        #endregion

        public static DateTime ToDate(this long ticks)
        {
            return new DateTime(ticks, DateTimeKind.Local);
        }
    }
}
