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
        [WebMethod(EnableSession = true)]
        //根据用户名和密码，返回用户id
        public void login(string uuid)
        {
            string huanxinUUID = "huanxin_" + uuid;
            string huanxinPWD = "huanxin_pwd_" + uuid;

            string loginResut = XinManager.instance.AccountGet(uuid);
            if (!IsRegisted(loginResut))
            {
                loginResut = XinManager.instance.AccountCreate(uuid, uuid);
            }

            string result = "";
            if (!IsRegisted(loginResut))
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
                string ss = XinManager.instance.UserWelcome(uuid);
            }
            Send(result);
        }

        bool IsRegisted(string jsonResult)
        {
            return jsonResult.Contains("uuid");
        }


        [WebMethod(EnableSession = true)]
        public void startMatch(int type, int day, int timeStart, int timeEnd, float latitude, float longitude, int level, string invite)
        {
            string uuid = Context.Session["uuid"].ToString();

            SportMatchCondition condition = new SportMatchCondition();
            condition.time = SportTime.From(day, timeStart, timeEnd);
            condition.sportType = type;
            condition.location = new Location(longitude, latitude);
            condition.level = level;
            condition.SetInviteList(uuid, invite);

            SportMatchUser user = new SportMatchUser();
            user.uuid = uuid;
            user.condition = condition;
            SportMatchManager.instance.AddMatchUser(user);

            Send(JsonGen.MatchResult("-1"));
        }

        [WebMethod(EnableSession = true)]
        public void getMatch()
        {
            string uuid = Context.Session["uuid"].ToString();
            string groupId = SportMatchManager.instance.GetUserGroupID(uuid);
            Send(JsonGen.MatchResult(groupId));
        }

        [WebMethod(EnableSession = true)]
        public void endMatch()
        {
            string uuid = Context.Session["uuid"].ToString();
            SportMatchManager.instance.StopMatch(uuid);
            Send(JsonGen.Status(100));
        }

        [WebMethod(EnableSession = true)]
        public void accept(string inviteUID)
        {
            string uuid = Context.Session["uuid"].ToString();
            if (SportMatchManager.instance.IsInGroup(uuid))
            {
                Send(JsonGen.Status(110));
                return;
            }

            string groupId = SportMatchManager.instance.SolveAcceptInvite(inviteUID, uuid);
            Send(JsonGen.Status(100));
        }


        public void Send(SimpleJSON.JSONClass jc)
        {
            Send(jc.ToJSON(0));
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
