using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class AthleteModel
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string gender;
        public string Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        private string idNumer;
        public string IdNumber
        {
            get { return idNumer; }
            set { idNumer = value; }
        }
    }
}
