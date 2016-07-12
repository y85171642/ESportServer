using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space.domain
{
    public class User
    {
        public string uuid;
        public string nickname;
        public string deviceID;
        public string huanxinID;
        public string huanxinPwd;

        public User(string uuid, string name, string deviceID, string huanxinID, string huanxinPwd)
        {
            this.uuid = uuid;
            this.nickname = name;
            this.deviceID = deviceID;
            this.huanxinID = huanxinID;
            this.huanxinPwd = huanxinPwd;
        }
    }
}