using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class SessionCollection : ObservableCollection<SessionModel>
    {
        public SessionCollection()
        {
        }

        public SessionCollection(IEnumerable<SessionModel> collection) : base(collection)
        {
        }
    }

    public class SessionModel
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

        private double weigth;
        public double Weight
        {
            get { return weigth; }
            set { weigth = value; }
        }

        private long height;
        public long Height
        {
            get { return height; }
            set { height = value; }
        }

        private bool footwear;
        public bool Footwear
        {
            get { return footwear; }
            set { footwear = value; }
        }

        private double treadmillSpeed;
        public double TreadmillSpeed
        {
            get { return treadmillSpeed; }
            set { treadmillSpeed = value; }
        }

        private string date;
        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        private long duration;
        public long Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        private string hash;
        public string GetHash()
        {
            return hash;
        }

        private string path;
        public string GetPath()
        {
            return path;
        }

        public SessionModel() { }

        public SessionModel(DB.Session DBSession)
        {
            hash = DBSession.session_hash;
            Name = DBSession.Athlete == null || string.IsNullOrWhiteSpace(DBSession.Athlete.name) ? "" : DBSession.Athlete.name;
            Gender = DBSession.Athlete == null || string.IsNullOrWhiteSpace(DBSession.Athlete.gender) ? "" : DBSession.Athlete.gender;
            Weight = DBSession.weight;
            Height = DBSession.height;
            Footwear = DBSession.footwear == 1;
            TreadmillSpeed = DBSession.treadmill_speed;
            Date = DateTime.Parse(DBSession.date).ToLocalTime().ToString();
            Duration = DBSession.duration;
            path = System.IO.Path.Combine(Environment.CurrentDirectory, DBSession.pathtoframes);
        }
    }
}
