using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.mysql
{
    public class ConnectManager
    {
        private static ConnectManager s_instance;
        public static ConnectManager instance()
        {
            return s_instance;
        }
        
        public MysqlConnection Conn
        {
            get
            {
                if(null ==  m_conn && !InitConnect())
                {
                    return null;
                }

                return m_conn;
            }
        }
        private MysqlConnection m_conn = null;
        ////
        private bool InitConnect()
        {
            m_conn = new MysqlConnection("127.0.0.1", "root", "x5", "takeout");
            string strInfo = m_conn.MysqlInfo();
            return strInfo != null;
        }
    }
}