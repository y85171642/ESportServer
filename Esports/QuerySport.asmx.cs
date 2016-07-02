using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace Esports
{
    /// <summary>
    /// QuerySport 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class QuerySport : System.Web.Services.WebService
    {

        [WebMethod]
        public int Query(string sport, DateTime time, float lat, float lon)
        {
            //需要一个match服务
            //当有一个运动的请求进来时，除了存储数据库以外，同时暂存在match的管理器中
            //第一个请求的玩家激活match的匹配线程，线程直道所有人匹配完成后结束（理论上不结束，除非停服）
            return 0;
        }
    }
}
