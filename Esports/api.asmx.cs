using Esports.space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace Esports
{
    /// <summary>
    /// api 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class api : System.Web.Services.WebService
    {
        public api()
        {
            //init
            SportMatchManager.instance.Start();
        }

        [WebMethod(EnableSession = true)]
        //根据用户名和密码，返回用户id
        public void login(string uuid)
        {
            string huanxinUUID = "huanxin_" + uuid;
            string huanxinPWD = "huanxin_pwd_" + uuid;

            string loginResut = XinManager.instance.AccountGet(huanxinUUID);
            if (!IsLogin(loginResut))
            {
                loginResut = XinManager.instance.AccountCreate(huanxinUUID, huanxinPWD);
            }

            string result = "";
            if (!IsLogin(loginResut))
            {
                result = JsonGen.Login(JsonGen.UserInfo(), 400).ToJSON(0);
            }
            else
            {
                result = JsonGen.Login(JsonGen.UserInfo(
                    uuid,
                    "nikeName",
                    "userHandicon",
                    huanxinUUID,
                    huanxinPWD,
                    SportMatchManager.instance.GetUserState(uuid),
                    SportMatchManager.instance.GetUserGroupID(uuid))
                    ).ToJSON(0);
                Context.Session["uuid"] = uuid;
            }
            Send(result);
        }

        bool IsLogin(string jsonResult)
        {
            return jsonResult.Contains("uuid");
        }


        [WebMethod(EnableSession = true)]
        public void startMatch(int type, int day, int timeStart, int timeEnd, float latitude, float longitude, int level, string invite)
        {
            SportMatchCondition condition = new SportMatchCondition();
            condition.time = SportTime.From(day, timeStart, timeEnd);
            condition.sportType = type;
            condition.location = new Location(longitude, latitude);
            condition.level = level;

            string uuid = Context.Session["uuid"].ToString();
            SportMatchUser user = new SportMatchUser();
            user.uuid = uuid;
            user.condition = condition;

            SportMatchManager.instance.AddMatchUser(user);

            string json = JsonGen.MatchResult("-1").ToJSON(0);
            Send(json);
        }

        [WebMethod(EnableSession = true)]
        public void getMatch()
        {
            string uuid = Context.Session["uuid"].ToString();
            SportMatchResult result = SportMatchManager.instance.GetMatchResultByUser(uuid);
            string groupId = result.isSuccess ? result.groupID : "-1";
            string json = JsonGen.MatchResult(groupId).ToJSON(0);
            Send(json);
        }

        public void Send(string data)
        {
            Context.Response.ContentType = "application/json";
            Context.Response.Charset = "utf-8"; //设置字符集类型
            Context.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Context.Response.Write(data);
            Context.Response.End();
        }
    }
}
