using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space.domain
{
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
}