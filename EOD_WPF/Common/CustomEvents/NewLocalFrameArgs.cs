using Common.KinectUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Utils
{
    public class NewLocalFrameArgs : INewFrameArgs
    {
        public BitmapSource Frame { get; set; }
    }
}
