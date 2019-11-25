using Server.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Server.Views
{
    /// <summary>
    /// Interaction logic for PlaySession.xaml
    /// </summary>
    public partial class PlaySession : Window
    {
        public event EventHandler WindowClosed;

        public PlaySession(Models.SessionModel session)
        {
            InitializeComponent();
            DataContext = new PlaySessionViewModel(this, session);
        }

        public void CloseAndNotify()
        {
            WindowClosed?.Invoke(this, null);
            Close();
        }
    }
}
