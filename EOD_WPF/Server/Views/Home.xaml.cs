using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Server.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.CurrentDirectory);
            if(!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "StoredFrames")))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.CurrentDirectory, "StoredFrames"));
            }
            if(!System.IO.File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "DB", "db.db")))
            {
                System.IO.File.Copy(System.IO.Path.Combine(Environment.CurrentDirectory, "DB", "donotdeleteme.db"), System.IO.Path.Combine(Environment.CurrentDirectory, "DB", "db.db"));
            }
        }

        private void Session(object sender, RoutedEventArgs e)
        {
            Session session = new Session();
            session.Show();
        }

        private void DataHistory(object sender, RoutedEventArgs e)
        {
            DataHistory datahistory = new DataHistory();
            datahistory.Show();
        }
    }
}
