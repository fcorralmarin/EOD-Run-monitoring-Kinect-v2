using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using Common.Utils;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System;

namespace Common.KinectUtils
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum POV
    {
        [EnumMember(Value = "Unkown")]
        Unknown,
        [EnumMember(Value = "Left")]
        Left,
        [EnumMember(Value = "Right")]
        Right
    }

    public class Gait
    {
        public POV POV { get;  set; }
        public double RelaxedAnkleFlexion { get;  set; }        
        public double CurrentAnkleFlexion { get;  set; }
        public double CurrentKneeFlexion { get;  set; }
        public double CurrentHipFlexion { get;  set; }

        public Gait()
        {
            POV = POV.Unknown;
        }

        public Gait(Gait gait)
        {
            CurrentHipFlexion = gait.CurrentHipFlexion;
            CurrentAnkleFlexion = gait.CurrentAnkleFlexion;
            CurrentKneeFlexion = gait.CurrentKneeFlexion;
            POV = gait.POV;
            RelaxedAnkleFlexion = gait.RelaxedAnkleFlexion;
        }

        public Gait(Skeleton avgSkeleton, POV pov) 
        {
            POV = pov;
            JointType foot = POV == POV.Left ? JointType.FootLeft : JointType.FootRight;
            JointType ankle = POV == POV.Left ? JointType.AnkleLeft : JointType.AnkleRight;
            JointType knee = POV == POV.Left ? JointType.KneeLeft : JointType.KneeRight;
            RelaxedAnkleFlexion = 90 - avgSkeleton.Joints[ankle].Angle(avgSkeleton.Joints[foot], avgSkeleton.Joints[knee]);
        }

        public void Refresh(Skeleton skeleton)
        {
            JointType foot = POV == POV.Left ? JointType.FootLeft : JointType.FootRight;
            JointType ankle = POV == POV.Left ? JointType.AnkleLeft : JointType.AnkleRight;
            JointType knee = POV == POV.Left ? JointType.KneeLeft : JointType.KneeRight;
            JointType hip = POV == POV.Left ? JointType.HipLeft : JointType.HipRight;
            JointType spineBase = JointType.SpineBase;
            JointType spineTop = JointType.SpineShoulder;
            UpdateCurrentValues(skeleton.Joints[foot], skeleton.Joints[ankle], skeleton.Joints[knee], skeleton.Joints[hip], skeleton.Joints[spineBase], skeleton.Joints[spineTop]);
        }
        private void UpdateCurrentValues(Joint foot, Joint ankle, Joint knee, Joint hip, Joint spineBase, Joint spineTop)
        {
            UpdateAnkleFlexion(foot, ankle, knee);
            UpdateKneeFlexion(ankle, knee, hip);
            UpdateHipFlexion(knee, hip, spineBase, spineTop);
        }
        private void UpdateAnkleFlexion(Joint foot, Joint ankle, Joint knee)
        {
            double totalAngle = ankle.Angle(foot, knee);
            CurrentAnkleFlexion = 90 - totalAngle - RelaxedAnkleFlexion; 
        }
        private void UpdateKneeFlexion(Joint ankle, Joint knee, Joint hip)
        {
            CurrentKneeFlexion = 180 - knee.Angle(ankle, hip);
        }
        private void UpdateHipFlexion(Joint knee, Joint hip, Joint spineBase, Joint spineTop)
        {
            Vector3D hipDisplacement = hip.BoneVector3(spineBase);
            spineTop.Position = Vector3D.Subtract(spineTop.Position.ToVector(), hipDisplacement).ToCamera();
            CurrentHipFlexion = 180 - hip.Angle(knee, spineTop);
        }
    }
}
