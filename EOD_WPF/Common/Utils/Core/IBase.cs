using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils.Core
{
    public class IBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object lockObj = new object();

        protected void RaisePropertyChanged(string propertyName)
        {
            lock (lockObj)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
