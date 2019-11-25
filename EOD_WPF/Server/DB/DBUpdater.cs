using Common.Utils;
using Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.DB
{
    public static class DBUpdater
    {
        public static void DeleteFromSelection(SessionCollection sessions)
        {
            using (DBEntities db = new DBEntities())
            {
                List<string> hashes = sessions.Select(x => x.GetHash()).ToList();
                List<string> paths = sessions.Select(x => x.GetPath()).Where(x => File.Exists(x)).ToList();
                IEnumerable<Session> dbSessions = db.Session.Where(x => hashes.Contains(x.session_hash));
                IEnumerable<Frame> dbFrames = dbSessions.SelectMany(x => x.Frame);
                db.Frame.RemoveRange(dbFrames);
                db.Session.RemoveRange(dbSessions);
                db.SaveChanges();
                paths.ForEach(x => File.Delete(x));
            }
        }

        public static void DeleteFromHash(string hash)
        {
            using(DBEntities db = new DBEntities())
            {
                db.Frame.RemoveRange(db.Frame.Where(x => x.session_hash == hash));
                db.Session.Remove(db.Session.FirstOrDefault(x => x.session_hash == hash));
                db.SaveChanges();
            }
        }

        public static Models.SessionModel CreateUnsavedSession()
        {
            Session newSession = new Session()
            {
                date = DateTime.UtcNow.ToString(),
                session_hash = DBHelper.GetNextId(),
                duration = 0,
                footwear = 0,
                height = 0,
                pathtoframes = "",
                saved = 0,
                treadmill_speed = 0,
                weight = 0,
                athlete_hash = ""
            };
            newSession.pathtoframes = Path.Combine("StoredFrames", newSession.session_hash + ".txt");
            using (DBEntities db = new DBEntities())
            {
                db.Session.Add(newSession);
                db.SaveChanges();
            }
            return new Models.SessionModel(newSession);
        }

        public static bool SaveSession(Models.SessionModel session, string athleteHash)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string hash = session.GetHash();
                    Session sessionToUpdate = db.Session.FirstOrDefault(x => x.session_hash == hash);
                    sessionToUpdate.footwear = session.Footwear ? 1 : 0;
                    sessionToUpdate.height = session.Height;
                    sessionToUpdate.treadmill_speed = session.TreadmillSpeed;
                    sessionToUpdate.weight = session.Weight;
                    sessionToUpdate.athlete_hash = athleteHash;
                    sessionToUpdate.duration = session.Duration;
                    sessionToUpdate.saved = 1;
                    db.SaveChanges();
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static string AddOrUpdateAthlete(Models.AthleteModel athlete)
        {
            Athlete newAthlete;
            using (DBEntities db = new DBEntities())
            {
                newAthlete = db.Athlete.FirstOrDefault(x => x.identification_number == athlete.IdNumber);
                if(newAthlete == null)
                {
                    newAthlete = new Athlete()
                    {
                        athlete_hash = DBHelper.GetNextId(),
                        gender = athlete.Gender,
                        identification_number = athlete.IdNumber,
                        name = athlete.Name
                    };
                    db.Athlete.Add(newAthlete);
                }
                else
                {
                    newAthlete.name = athlete.Name;
                    newAthlete.gender = athlete.Gender;
                }
                db.SaveChanges();
            }
            return newAthlete.athlete_hash;
        }

        public static void DigestSession(Models.SessionModel session, ref Queue<NewRemoteFrameArgs> localFrames, ref Queue<NewRemoteFrameArgs> remoteFrames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            DateTime firstInstant = Math.Max(localFrames.First().TimeStamp.ToDate().AddSeconds(1).Ticks, remoteFrames.First().TimeStamp.ToDate().AddSeconds(1).Ticks).ToDate();
            DateTime lastInstant = Math.Min(localFrames.Last().TimeStamp.ToDate().Ticks, remoteFrames.Last().TimeStamp.ToDate().Ticks).ToDate();

            localFrames = new Queue<NewRemoteFrameArgs>(localFrames.SkipWhile(x => x.TimeStamp.ToDate().Second != firstInstant.Second)
                                                                   .TakeWhile(x => x.TimeStamp.ToDate().Minute != lastInstant.Minute || x.TimeStamp.ToDate().Second != lastInstant.Second));
            remoteFrames = new Queue<NewRemoteFrameArgs>(remoteFrames.SkipWhile(x => x.TimeStamp.ToDate().Second != firstInstant.Second)
                                                                     .TakeWhile(x => x.TimeStamp.ToDate().Minute != lastInstant.Minute || x.TimeStamp.ToDate().Second != lastInstant.Second));

            DBHelper.InsertFrames(session.GetHash(), DBHelper.DigestFrames(true, ref localFrames, ref stringBuilder), true);
            DBHelper.InsertFrames(session.GetHash(), DBHelper.DigestFrames(false, ref remoteFrames, ref stringBuilder), false);
            File.WriteAllBytes(session.GetPath(), stringBuilder.ToString().Zip());
        }
    }
}
