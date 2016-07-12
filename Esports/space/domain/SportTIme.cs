using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Esports.space.domain
{
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