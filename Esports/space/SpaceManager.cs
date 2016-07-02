using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space
{
    public class SportSpace
    {
        public string name;
        public float lat;
        public float lon;
        public int price;
        public string desc;
        public string[] flags;//运动标签 跑步，羽毛球，乒乓球，篮球..


    }
    public class SpaceManager
    {
        
        private static SpaceManager m_instance = new SpaceManager();
        public static SpaceManager instance()
        {
            return m_instance;
        }

        //从数据库读取数据
        public void Init()
        {
        }

        public List<SportSpace> GetSpace(float lat, float lon)
        {
            return null;
        }
    }
}