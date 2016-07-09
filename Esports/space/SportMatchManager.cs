using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Esports.space
{
    public class SportMatchManager
    {
        private static SportMatchManager s_instance;
        public static SportMatchManager instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new SportMatchManager();
                return s_instance;
            }
        }

        private List<SportMatchGroup> groupList = new List<SportMatchGroup>();
        private List<SportMatchUser> waitingUserList = new List<SportMatchUser>();
        private Dictionary<string, string> userGroupDict = new Dictionary<string, string>();
        public void AddMatchUser(SportMatchUser condition)
        {
            Monitor.Enter(waitingUserList);
            try
            {
                waitingUserList.Add(condition);
            }
            finally
            {
                Monitor.Exit(waitingUserList);
            }
        }

        public void StopMatch(string uuid)
        {
            Monitor.Enter(waitingUserList);
            try
            {
                SportMatchUser user = waitingUserList.Find(a => { return a.uuid == uuid; });
                if (user != null)
                    waitingUserList.Remove(user);
            }
            finally
            {
                Monitor.Exit(waitingUserList);
            }
        }

        public void RemoveFormGroup(string uuid)
        {
            Monitor.Enter(userGroupDict);
            try
            {
                if (userGroupDict.ContainsKey(uuid))
                    userGroupDict.Remove(uuid);
            }
            finally
            {
                Monitor.Exit(userGroupDict);
            }
        }

        public int GetUserState(string uuid)
        {
            Monitor.Enter(userGroupDict);
            Monitor.Enter(waitingUserList);
            try
            {
                if (userGroupDict.ContainsKey(uuid))
                    return 2;
                else if (waitingUserList.Any(a => { return a.uuid == uuid; }))
                    return 1;
                else
                    return 0;
            }
            finally
            {
                Monitor.Exit(userGroupDict);
                Monitor.Exit(waitingUserList);
            }
        }

        public string GetUserGroupID(string uuid)
        {
            Monitor.Enter(userGroupDict);
            try
            {
                if (userGroupDict.ContainsKey(uuid))
                    return userGroupDict[uuid];
                return "-1";
            }
            finally
            {
                Monitor.Exit(userGroupDict);
            }
        }

        public bool IsInGroup(string uuid)
        {
            Monitor.Enter(userGroupDict);
            try
            {
                if (userGroupDict.ContainsKey(uuid))
                    return true;
                return false;
            }
            finally
            {
                Monitor.Exit(userGroupDict);
            }
        }

        public string SolveAcceptInvite(string from, string to)
        {
            Monitor.Enter(waitingUserList);
            Monitor.Enter(groupList);
            try
            {
                if (IsInGroup(from))
                {
                    string groupID = GetUserGroupID(from);
                    SportMatchGroup group = groupList.Find(a => { return a.groupID == groupID; });
                    SportInvite inv = group.condition.inviteList.Find(b => { return b.fromUUID == from; });
                    inv.isAccepted = true;
                    return JoinGroup(to, GetUserGroupID(from));
                }
                else
                {
                    SportMatchUser user = waitingUserList.Find(a => { return a.uuid == from; });
                    SportInvite inv = user.condition.inviteList.Find(b => { return b.fromUUID == from; });
                    inv.isAccepted = true;
                    return "-1";
                }
            }
            finally
            {
                Monitor.Exit(waitingUserList);
                Monitor.Exit(groupList);
            }
        }

        public string JoinGroup(string uuid, string groupId)
        {
            Monitor.Enter(userGroupDict);
            Monitor.Enter(waitingUserList);
            try
            {
                SportMatchUser user = waitingUserList.Find(a => { return a.uuid == uuid; });
                if (user != null)
                    waitingUserList.Remove(user);
                if (!userGroupDict.ContainsKey(uuid))
                {
                    userGroupDict.Add(uuid, groupId);
                    XinManager.instance.GroupJoin(groupId, uuid);
                    SendGroupWelcome(groupId, uuid);
                }
                else
                {
                    //TODO: 在别的群里
                    // userGroupDict[uuid] = groupId;
                }
                return userGroupDict[uuid];
            }
            finally
            {
                Monitor.Exit(userGroupDict);
                Monitor.Exit(waitingUserList);
            }
        }


        public void Start()
        {
            new Thread(new ThreadStart(MatchLooper)).Start();
        }

        private void MatchLooper()
        {
            while (true)
            {
                DoMatch();
                Thread.Sleep(1000);
            }
        }

        private void DoMatch()
        {
            Monitor.Enter(waitingUserList);
            try
            {
                DoMatchSync();
            }
            finally
            {
                Monitor.Exit(waitingUserList);
            }
        }

        private void DoMatchSync()
        {
            // 优先Group匹配
            for (int i = waitingUserList.Count - 1; i >= 0; i--)
            {
                SportMatchUser user = waitingUserList[i];
                foreach (SportMatchGroup group in groupList)
                {
                    if (user.condition.IsMatch(group.condition))
                    {
                        group.UpdateGroupCondition(user.condition);
                        JoinGroup(user.uuid, group.groupID);
                        break;
                    }
                }
            }

            // 没匹配到的用户，进行用户间匹配
            for (int i = waitingUserList.Count - 1; i >= 0; i--)
            {
                SportMatchUser user = waitingUserList[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    SportMatchUser user2 = waitingUserList[j];
                    if (user.condition.IsMatch(user2.condition))
                    {
                        SportMatchGroup group = SportMatchGroup.CreateGroup(user.condition);
                        AddGroup(group);    // 创建Group
                        return;
                    }
                }
            }
        }

        private void AddGroup(SportMatchGroup group)
        {
            Monitor.Enter(groupList);
            try
            {
                groupList.Add(group);
            }
            finally
            {
                Monitor.Exit(groupList);
            }
        }

        private void SendGroupWelcome(string groupID, string targetUUID)
        {
            XinManager.instance.GroupWelcome(groupID, targetUUID);
        }
    }

    public class SportMatchGroup
    {
        public string groupID;
        public SportMatchCondition condition;

        public static SportMatchGroup CreateGroup(SportMatchCondition condition)
        {
            string groupID = CreateGroupID();
            SportMatchGroup group = new SportMatchGroup();
            group.groupID = groupID;
            SportMatchCondition condi = new SportMatchCondition();
            condi.level = condition.level;
            condi.location = condition.location;
            condi.sportType = condition.sportType;
            condi.time = condition.time;
            group.condition = condi;
            return group;
        }

        public static string CreateGroupID()
        {
            string json = XinManager.instance.GroupCreate();
            SimpleJSON.JSONNode jn = SimpleJSON.JSON.Parse(json);
            string groupId = jn["data"]["groupid"];
            return groupId;
        }

        public void UpdateGroupCondition(SportMatchCondition upCondition)
        {
            // TODO: 根据新匹配条件 更新当前匹配条件
            UpdateGroupInivteList(upCondition);
        }

        public void UpdateGroupInivteList(SportMatchCondition upCondition)
        {
            foreach (SportInvite si in upCondition.inviteList)
            {
                SportInvite inv = condition.inviteList.Find((a) => { return a.toUUID == si.toUUID; });
                if (inv == null)
                    condition.inviteList.Add(si);
                else
                    inv.isAccepted = si.isAccepted;

                SendInvite(si.fromUUID, si.toUUID);
            }
        }

        private void SendInvite(string from, string to)
        {
            XinManager.instance.SendInvite(from, to);
        }
    }

    public class SportMatchUser
    {
        public string uuid;
        public SportMatchCondition condition;
    }

    public class SportInvite
    {
        public string fromUUID;
        public string toUUID;
        public bool isAccepted;
    }

    public class SportMatchCondition
    {
        public int sportType;
        public Location location;
        public SportTime time;
        public int level;

        public List<SportInvite> inviteList = new List<SportInvite>();

        public bool IsMatch(SportMatchCondition condition)
        {
            if (sportType == condition.sportType
                && Location.Distance(location, condition.location) < 2000
                && time.IsOverLapWith(condition.time)
                && Math.Abs(level - condition.level) <= 2)
            {
                return true;
            }
            return false;
        }

        public void SetInviteList(string from, string inviteListStr)
        {
            inviteListStr = inviteListStr.Trim();
            inviteListStr = inviteListStr.Trim(',');
            string[] inviteArray = inviteListStr.Split(',');
            foreach (string inviteUUID in inviteArray)
            {
                if (!inviteList.Any(a => { return a.toUUID == inviteUUID; }))
                {
                    SportInvite si = new SportInvite();
                    si.fromUUID = from;
                    si.toUUID = inviteUUID;
                    si.isAccepted = false;
                    inviteList.Add(si);
                }
            }
        }
    }

    public class Location
    {
        /// <summary>
        /// 经度
        /// </summary>
        public float longitude;

        /// <summary>
        /// 维度
        /// </summary>
        public float latitude;

        public Location(float lon, float lat)
        {
            longitude = lon;
            latitude = lat;
        }

        public static float Distance(Location a, Location b)
        {
            return (float)Distance(a.latitude, a.longitude, b.latitude, b.longitude);
        }

        private const double EARTH_RADIUS = 6378.137;//地球半径
        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        public static double Distance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
    }

    public class SportTime
    {
        public int from;
        public int to;

        /// <summary>
        /// Create Sport Time From Week Time
        /// </summary>
        /// <param name="dayOfWeek">dayOfWeek</param>
        /// <param name="from">hourOfDay</param>
        /// <param name="to">hourOfDay</param>
        public static SportTime From(int dayOfWeek, int from, int to)
        {
            SportTime st = new SportTime();
            st.from = dayOfWeek * 24 + from;
            st.to = dayOfWeek * 24 + to;
            return st;
        }

        public bool IsOverLapWith(SportTime time)
        {
            return OverLapLength(time) > 0;
        }

        /// <summary>
        /// 重合长度
        /// </summary>
        /// <returns></returns>
        public int OverLapLength(SportTime time)
        {
            int maxFrom = from > time.from ? from : time.from;
            int minTo = to > time.to ? time.to : to;
            return minTo - maxFrom;
        }
    }
}