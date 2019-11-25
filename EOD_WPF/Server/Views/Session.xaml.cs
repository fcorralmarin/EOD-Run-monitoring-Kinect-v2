using Common;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Server
{
    /// <summary>
    /// Interaction logic for Session.xaml
    /// </summary>
    public partial class Session : Window
    {
        public Session()
        {
            this.InitializeComponent();            
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
    }
}
