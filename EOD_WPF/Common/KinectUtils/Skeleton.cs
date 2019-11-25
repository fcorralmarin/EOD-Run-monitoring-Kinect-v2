using Microsoft.Kinect;
using Common.Utils;
using System;
using System.Collections.Generic;

namespace Common.KinectUtils
{
    public class Skeleton
    {
        public bool IsTracked = false;
        internal Dictionary<JointType, Joint> Joints = new Dictionary<JointType, Joint>();

        public Skeleton()
        {
            Joints = new Dictionary<JointType, Joint>();
        }

        public Skeleton(Body body)
        {
            if (body != null)
            {
                IsTracked = body.IsTracked;
                if (body.Joints != null)
                {
                    Joints = body.Joints as Dictionary<JointType, Joint>;
                }
            }
        }
    }
}
