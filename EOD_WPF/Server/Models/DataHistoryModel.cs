using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Server.Models
{
    public class DataHistoryModel
    {
        private DataGrid dataGrid;
        public DataGrid DataGrid
        {
            get { return dataGrid; }
            set { dataGrid = value; }
        }

        private ComboBoxItem gender;
        public ComboBoxItem Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        private DateTime? from;
        public DateTime? From
        {
            get { return from; }
            set { from = value; }
        }

        private DateTime? to;
        public DateTime? To
        {
            get { return to; }
            set { to = value; }
        }

        private string search;
        public string Search
        {
            get { return search; }
            set { search = value; }
        }
    }
}
