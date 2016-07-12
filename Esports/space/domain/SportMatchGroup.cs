using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space.domain
{
    public class SportMatchGroup
    {
        public string groupID;
        public SportMatchCondition condition;
        public Venue venue = null;

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

        public bool SetVenue(Venue venue)
        {
            if (this.venue != null)
                return false;
            this.venue = venue;
            return true;
        }

        public void RemoveVenue()
        {
            this.venue = null;
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
}