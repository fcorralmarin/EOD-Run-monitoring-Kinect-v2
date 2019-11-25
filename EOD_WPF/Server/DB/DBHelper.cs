using Common.KinectUtils;
using Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    internal static class DBHelper
    {

        private static readonly string EncodeChars = "0123456789ABCDEFGHIJKLMNOPQRSTUV";

        private static long LastID = DateTime.UtcNow.Ticks;

        internal static string GetNextId() => GenerateId(System.Threading.Interlocked.Increment(ref LastID));

        private static unsafe string GenerateId(long id)
        {
            char* charBuffer = stackalloc char[13];

            charBuffer[0] = EncodeChars[(int)(id >> 60) & 31];
            charBuffer[1] = EncodeChars[(int)(id >> 55) & 31];
            charBuffer[2] = EncodeChars[(int)(id >> 50) & 31];
            charBuffer[3] = EncodeChars[(int)(id >> 45) & 31];
            charBuffer[4] = EncodeChars[(int)(id >> 40) & 31];
            charBuffer[5] = EncodeChars[(int)(id >> 35) & 31];
            charBuffer[6] = EncodeChars[(int)(id >> 30) & 31];
            charBuffer[7] = EncodeChars[(int)(id >> 25) & 31];
            charBuffer[8] = EncodeChars[(int)(id >> 20) & 31];
            charBuffer[9] = EncodeChars[(int)(id >> 15) & 31];
            charBuffer[10] = EncodeChars[(int)(id >> 10) & 31];
            charBuffer[11] = EncodeChars[(int)(id >> 5) & 31];
            charBuffer[12] = EncodeChars[(int)id & 31];

            return new string(charBuffer, 0, 13);
        }

        internal static void InsertFrames(string sessionHash, List<KeyValuePair<long, Gait>> gaits, bool isLocal)
        {
            List<Frame> framesToInsert = new List<Frame>();
            framesToInsert = gaits.Select(x => new Frame()
            {
                timestamp = x.Key,
                is_local_device = isLocal ? 1 : 0,
                serialized_object = JsonConvert.SerializeObject(x.Value),
                session_hash = sessionHash
            }).ToList();
            DBEntities db = null;
            try
            {
                db = new DBEntities();
                db.Frame.AddRange(framesToInsert);
                db.SaveChanges();
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
        }

        internal static List<KeyValuePair<long, Gait>> DigestFrames(bool isLocal, ref Queue<NewRemoteFrameArgs> frames, ref StringBuilder stringBuilder)
        {
            List<KeyValuePair<long, Gait>> gaits = new List<KeyValuePair<long, Gait>>();
            int index = 0;
            NewRemoteFrameArgs previous = frames.Dequeue();
            NewRemoteFrameArgs current;
            while (frames.Count > 0)
            {
                current = frames.Dequeue();
                if (string.IsNullOrWhiteSpace(previous.Frame))
                {
                    previous.Frame = current.Frame;
                }
                stringBuilder.AppendFormat("{0}@{1}{2}", isLocal ? 1 : 0, previous.Frame, Environment.NewLine);
                gaits.Add(new KeyValuePair<long, Gait>(index, previous.Gait));
                if (previous.FPS == 15)
                {
                    index++;
                    stringBuilder.AppendFormat("{0}@{1}{2}", isLocal ? 1 : 0, previous.Frame, Environment.NewLine);
                    gaits.Add(new KeyValuePair<long, Gait>(index, previous.Gait.AvgGait(current.Gait)));
                }
                previous = current;
                index++;
            }
            if (string.IsNullOrWhiteSpace(previous.Frame))
            {
                stringBuilder.AppendFormat("{0}@{1}{2}", isLocal ? 1 : 0, previous.Frame, Environment.NewLine);
                gaits.Add(new KeyValuePair<long, Gait>(index, previous.Gait));
            }
            if (previous.FPS == 15)
            {
                index++;
                stringBuilder.AppendFormat("{0}@{1}{2}", isLocal ? 1 : 0, previous.Frame, Environment.NewLine);
                gaits.Add(new KeyValuePair<long, Gait>(index, previous.Gait));
            }
            return gaits;
        }
    }
}
