using Client.ViewModels;
using System;
using System.Windows;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for Client.xaml
    /// </summary>
    public partial class Client : Window
    {
        public event EventHandler WindowClosed;

        public Client()
        {
            InitializeComponent();
            DataContext = new ClientViewModel(this);
        }

        public void CloseWindow()
        {
            WindowClosed?.Invoke(this, null);
            Close();
        }
    }
}
