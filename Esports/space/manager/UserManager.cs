using Esports.space.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space.manager
{
    public class UserManager
    {
        private static UserManager s_instance;
        public static UserManager instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new UserManager();
                return s_instance;
            }
        }

        public List<User> userList = new List<User>();

        public void AddUser(string uuid, string name, string deviceID, string huanxinID, string huanxinPwd)
        {
            if (!userList.Any(a => { return a.uuid == uuid; }))
            {
                User user = new User(uuid, name, deviceID, huanxinID, huanxinPwd);
                userList.Add(user);
            }
        }

        public void ModifyName(string uuid, string nickname)
        {
            User user = userList.Find(a => { return a.uuid == uuid; });
            if (user != null)
                user.nickname = nickname;
        }

        public string GetUserNickname(string uuid)
        {
            User user = userList.Find(a => { return a.uuid == uuid; });
            if (user != null)
                return user.nickname;
            return uuid;
        }
    }
}