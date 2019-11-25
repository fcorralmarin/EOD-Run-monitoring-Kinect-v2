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
    /// Interaction logic for SaveSession.xaml
    /// </summary>
    public partial class SaveSession : Window
    {
        public event EventHandler<bool> WindowClosed;

        public SaveSession(Models.SessionModel session)
        {
            InitializeComponent();
            DataContext = new SaveSessionViewModel(this, session);
        }

        private void TextBox_PreviewTextInputInt(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int x);
        }

        private void TextBox_PreviewTextInputFloat(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                if (!((TextBox)sender).Text.Contains("."))
                    e.Handled = false;
                else
                    e.Handled = true;
            }
            else
            {
                e.Handled = !float.TryParse(e.Text, out float x);
            }
        }

        public void CloseAndNotify(bool saveSession)
        {
            WindowClosed?.Invoke(this, saveSession);
            Close();
        }
    }
}
