using HXComm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space
{
    public class XinManager
    {
        private static XinManager s_instance;
        public static XinManager instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new XinManager();
                return s_instance;
            }
        }

        private XinManager()
        {
            m_xin = new EaseXin(HuanXinConfig.CLIENT_ID, HuanXinConfig.CLIENT_SECRET, HuanXinConfig.APP_NAME, HuanXinConfig.ACCOUNT);
        }

        private EaseXin m_xin;

        public string AccountGet(string userName)
        {
            return m_xin.AccountGet("huanxin_" + userName);
        }

        public string AccountCreate(string userName, string pwd, string nickname)
        {
            return m_xin.AccountCreate("huanxin_" + userName, "huanxin_pwd_" + pwd, nickname);
        }

        public string AccountAddFriend(string from, string to)
        {
            return m_xin.AccountAddFriend("huanxin_" + from, "huanxin_" + to);
        }

        public string AccountGetFriends(string uuid)
        {
            return m_xin.AccountGetFriends("huanxin_" + uuid);
        }

        public string ModifyName(string uuid, string nickname)
        {
            return m_xin.ModifyNickName(uuid, nickname);
        }

        public string GroupCreate()
        {
            return m_xin.GroupCreate();
        }

        public string GroupJoin(string groupID, string uuid)
        {
            return m_xin.GroupJoin(groupID, "huanxin_" + uuid);
        }

        public string GroupExit(string groupID, string uuid)
        {
            return m_xin.GroupExit(groupID, "huanxin_" + uuid);
        }

        public string GroupWelcome(string groupID, string targetID)
        {
            return m_xin.SendGroupNotice(groupID, "欢迎 " + "huanxin_" + targetID + " 加入群组！");
        }

        public string GroupNotice(string groupID, string msg, string extJson = null)
        {
            return m_xin.SendGroupNotice(groupID, msg, extJson);
        }

        public string SendInvite(string from, string to)
        {
            return m_xin.SendUserInvite("huanxin_" + from, "huanxin_" + to);
        }

        public string UserWelcome(string uuid)
        {
            return m_xin.UserWelcome("huanxin_" + uuid);
        }

    }
}