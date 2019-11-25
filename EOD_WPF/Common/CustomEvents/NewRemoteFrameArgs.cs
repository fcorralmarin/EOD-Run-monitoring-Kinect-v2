using Common.KinectUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Common.Utils
{
    public class NewRemoteFrameArgs : INewFrameArgs
    {
        public string Frame { get; set; }
    }
}
