using Common.KinectUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Common.Utils
{
    public class NewLocalFrameArgs : EventArgs
    {
        public long TimeStamp { get; set; }
        public ImageSource Frame { get; set; }
        public int FPS { get; set; }
        public List<Skeleton> Bodies { get; set; }
    }
}
