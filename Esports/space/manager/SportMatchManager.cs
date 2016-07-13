using Esports.space.domain;
using Esports.space.manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using SimpleJSON;

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

        #region interface

        public void Start()
        {
            new Thread(new ThreadStart(MatchLooper)).Start();
        }

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

        public bool CheckGroupDismiss(string groupID)
        {
            Monitor.Enter(userGroupDict);
            Monitor.Enter(groupList);
            try
            {
                var itr = userGroupDict.GetEnumerator();
                while (itr.MoveNext())
                {
                    if (itr.Current.Value == groupID)
                        return false;
                }
                SportMatchGroup group = groupList.Find(a => { return a.groupID == groupID; });
                if (group == null)
                    return false;
                groupList.Remove(group);
                return true;
            }
            finally
            {
                Monitor.Exit(userGroupDict);
                Monitor.Exit(groupList);
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
                    if (inv != null) //待处理的邀请队列，群已创建后，好友邀请时不存在此项
                        inv.isAccepted = true;
                    return JoinGroup(to, GetUserGroupID(from));
                }
                else
                {
                    SportMatchUser user = waitingUserList.Find(a => { return a.uuid == from; });
                    if (user != null)
                    {
                        SportInvite inv = user.condition.inviteList.Find(b => { return b.fromUUID == from; });
                        if (inv != null)
                            inv.isAccepted = true;
                    }
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
                    SendGroupJoin(groupId, uuid);
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

        public bool DestineVenue(string uuid, Venue venue)
        {
            Monitor.Enter(groupList);
            try
            {
                string groupid = GetUserGroupID(uuid);
                SportMatchGroup group = groupList.Find(a => { return a.groupID == groupid; });
                if (group == null)
                    return false;
                return group.SetVenue(venue);
            }
            finally
            {
                Monitor.Exit(groupList);
            }
        }

        #endregion

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

        /*
         * {
                "type": "group_info", //标记
                "group": //group 信息
                 { 
                    "member": 
                    [//成员
                        {
                            "uid": ""//用户id
                        },
                        ...
                    ],
                    "vid": "", //场馆id
                    "name": "",//..名字
                    "address": "",//..地址
                    "time": "",//..时间
                    "latitude": "",//..经度
                    "longtitude": "",//..纬度
                    "state": "",//..状态 3种状态 0 是未预订 2是预订 1是投票状态
                    "max": ""//..最大成员数 默认10 
                }
            }
         */
        public string GetExtJson(string groupID)
        {
            Monitor.Enter(userGroupDict);
            try
            {
                SportMatchGroup group = groupList.Find(a => { return a.groupID == groupID; });
                Venue venue = group.venue;

                JSONClass jc = new JSONClass();
                jc.Add("type", new JSONData("group_info"));
                JSONClass jc_1 = new JSONClass();
                JSONArray ja_1_1 = new JSONArray();
                var itr = userGroupDict.GetEnumerator();
                while (itr.MoveNext())
                {
                    string groupid = itr.Current.Value;
                    if (groupid != groupID)
                        continue;
                    string uuid = itr.Current.Key;
                    JSONClass jc_1_1_i = new JSONClass();
                    jc_1_1_i.Add("uid", new JSONData(uuid));
                    ja_1_1.Add(jc_1_1_i);
                }
                jc_1.Add("member", ja_1_1);
                if (venue == null)
                {
                    jc_1.Add("vid", new JSONData(""));
                    jc_1.Add("name", new JSONData(""));
                    jc_1.Add("address", new JSONData(""));
                    jc_1.Add("time", new JSONData(""));
                    jc_1.Add("latitude", new JSONData(""));
                    jc_1.Add("longtitude", new JSONData(""));
                    jc_1.Add("state", new JSONData(0));
                }
                else
                {
                    jc_1.Add("vid", new JSONData(venue.id));
                    jc_1.Add("name", new JSONData(venue.name));
                    jc_1.Add("address", new JSONData(venue.address));
                    jc_1.Add("time", new JSONData(venue.time));
                    jc_1.Add("latitude", new JSONData(venue.latitude));
                    jc_1.Add("longtitude", new JSONData(venue.longitude));
                    jc_1.Add("state", new JSONData(2));
                }
                jc_1.Add("max", new JSONData(10));
                jc.Add("group", jc_1);
                return jc.ToJSON(0);
            }
            finally
            {
                Monitor.Exit(userGroupDict);
            }
        }

        public void SendGroupJoin(string groupID, string targetUUID)
        {
            Monitor.Enter(groupList);
            try
            {
                string msg = "欢迎 " + UserManager.instance.GetUserNickname(targetUUID) + " 加入群组！";
                string ext = GetExtJson(groupID);
                XinManager.instance.GroupNotice(groupID, msg, ext);
            }
            finally
            {
                Monitor.Exit(groupList);
            }
        }

        public void SendGroupDestineVenue(string groupID, string uuid)
        {
            Monitor.Enter(groupList);
            try
            {
                SportMatchGroup group = groupList.Find(a => { return a.groupID == groupID; });
                string nickname = UserManager.instance.GetUserNickname(uuid);
                string msg = nickname + "成功预订场馆。［" + group.venue.name + "］－［" + group.venue.address + "］";
                string ext = GetExtJson(groupID);
                XinManager.instance.GroupNotice(groupID, msg, ext);
            }
            finally
            {
                Monitor.Exit(groupList);
            }
        }

        public void SendGroupExit(string groupID, string targetUUID)
        {
            Monitor.Enter(groupList);
            try
            {
                string msg = UserManager.instance.GetUserNickname(targetUUID) + " 退出群组！";
                string ext = GetExtJson(groupID);
                XinManager.instance.GroupNotice(groupID, msg, ext);
            }
            finally
            {
                Monitor.Exit(groupList);
            }
        }
    }
}