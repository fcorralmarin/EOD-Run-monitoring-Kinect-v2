using Common.KinectUtils;
using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public static class DBAccessor
    {
        public static SessionCollection GetAllDBEntries()
        {
            using(DBEntities db = new DBEntities())
            {
                SessionCollection sessions = new SessionCollection();
                db.Session.RemoveRange(db.Session.Where(x => x.saved == 0));
                db.SaveChanges();
                List<Session> dbSessions = db.Session.Select(x => x).ToList();
                foreach(Session dbSession in dbSessions)
                {
                    sessions.Add(new Models.SessionModel(dbSession));
                }
                return sessions;
            }
        }

        public static SessionCollection GetFilteredDBEntries(string search, string gender, DateTime? from, DateTime? to)
        {
            SessionCollection result;
            using (DBEntities db = new DBEntities())
            {
                IEnumerable<Session> sessions = db.Session.Select(x => x);
                if (to.HasValue)
                {
                    sessions = sessions.Where(x => DateTime.Parse(x.date) <= to.Value);
                }
                if (from.HasValue)
                {
                    sessions = sessions.Where(x => DateTime.Parse(x.date) >= from.Value);
                }
                if(gender.ToUpper() != "BOTH")
                {
                    sessions = sessions.Where(x => x.Athlete.gender.ToUpper() == gender.ToUpper());
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    sessions = sessions.Where(x => x.Athlete.name.ToUpper().StartsWith(search.ToUpper()) ||
                                                   x.Athlete.identification_number.ToUpper() == search.ToUpper());
                }
                result = new SessionCollection(sessions.Select(x => new Models.SessionModel(x)));
            }
            return result;
        }        

        public static Models.AthleteModel GetAthleteIfExists(string idNumber)
        {
            Models.AthleteModel athlete = null;
            using(DBEntities db = new DBEntities())
            {
                if(db.Athlete.Any(x => x.identification_number == idNumber))
                {
                    athlete = db.Athlete.Where(x => x.identification_number == idNumber).Select(x => new Models.AthleteModel()
                    {
                        Gender = x.gender,
                        IdNumber = x.identification_number,
                        Name = x.name
                    }).First();
                }
            }
            return athlete;
        }

        public static string GetExistingAthleteHash(string idNumber)
        {
            string hash = "";
            using (DBEntities db = new DBEntities())
            {
                hash = db.Athlete.FirstOrDefault(x => x.identification_number == idNumber).athlete_hash;
            }
            return hash;
        }

        public static List<Gait> GetGaits(string sessionHash, bool isLocal)
        {
            List<Gait> gaits = new List<Gait>();
            using (DBEntities db = new DBEntities())
            {
                var serializedGaits = db.Frame.Where(x => x.session_hash == sessionHash && x.is_local_device == (isLocal ? 1 : 0)).Select(x => x.serialized_object).ToList();
                gaits = serializedGaits.Select(x => JsonConvert.DeserializeObject<Gait>(x)).ToList();
            }
            return gaits;
        }
    }
}
