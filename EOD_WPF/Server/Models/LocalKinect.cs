using Common.KinectUtils;
using Common.TCP;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Server.Models
{
    public class LocalKinect
    {
        private BitmapSource image;
        private KinectController kinectController;
        private bool prepared = false;

        public BitmapSource Image { get => image; set => image = value; }
        public KinectController KinectController { get => kinectController; set => kinectController = value; }
        public bool Prepared { get => prepared; set => prepared = value; }

        public LocalKinect()
        {
            KinectController = new KinectController(true);
        }
    }
}
