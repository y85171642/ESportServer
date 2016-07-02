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
            return m_xin.AccountGet(userName);
        }

        public string AccountCreate(string userName, string pwd)
        {
            return m_xin.AccountCreate(userName, pwd);
        }

        public string GroupCreate()
        {
            return m_xin.GroupCreate();
        }

    }
}