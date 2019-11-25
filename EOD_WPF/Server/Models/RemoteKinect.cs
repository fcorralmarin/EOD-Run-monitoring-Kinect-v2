using Common.KinectUtils;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Server.Models
{
    public class RemoteKinect
    {
        private BitmapSource image;
        private Common.TCP.Server server;
        private bool prepared = false;

        public BitmapSource Image { get => image; set => image = value; }
        public Common.TCP.Server Server { get => server; set => server = value; }
        public bool Prepared { get => prepared; set => prepared = value; }

        public RemoteKinect()
        {
        }
    }
}
