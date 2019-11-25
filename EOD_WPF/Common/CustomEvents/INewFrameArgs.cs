using Common.KinectUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public class INewFrameArgs
    {
        public long TimeStamp { get;  set; }
        public int FPS { get;  set; }
        public Gait Gait { get;  set; }
    }
}
