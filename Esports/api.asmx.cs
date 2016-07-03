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
        public void login(string uuid)
        {
            LOG.Out("LOGIN: " + uuid);

            uuid = DVCUID.DeviceIDToUUID(uuid);
            string huanxinUUID = "huanxin_" + uuid;
            string huanxinPWD = "huanxin_pwd_" + uuid;

            string loginResut = XinManager.instance.AccountGet(uuid);
            if (!IsRegisted(loginResut))
            {
                loginResut = XinManager.instance.AccountCreate(uuid, uuid);
                XinManager.instance.UserWelcome(uuid);
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
            }
            Send(result);
        }

        bool IsRegisted(string jsonResult)
        {
            return jsonResult.Contains("uuid");
        }

        bool IsAddFriendSuccess(string jsonResult)
        {
            return jsonResult.Contains("action");
        }


        [WebMethod(EnableSession = true)]
        public void addFriend(string target)
        {
            string uuid = Context.Session["uuid"].ToString();

            LOG.Out("AddFriend: " + uuid + " ===> " + target);

            string json = XinManager.instance.AccountAddFriend(uuid, target);
            if (IsAddFriendSuccess(json))
                Send(JsonGen.Status(100));
            else
                Send(JsonGen.Status(106));
        }


        [WebMethod(EnableSession = true)]
        public void getFriends()
        {
            string uuid = Context.Session["uuid"].ToString();
            LOG.Out("GetFriends: " + uuid);
            string json = XinManager.instance.AccountGetFriends(uuid);
            SimpleJSON.JSONArray friendArray = new SimpleJSON.JSONArray();
            SimpleJSON.JSONNode jn = SimpleJSON.JSON.Parse(json);
            SimpleJSON.JSONArray ja = jn["data"].AsArray;
            foreach (SimpleJSON.JSONData jd in ja)
            {
                string fuid = HuanXinIDToUID(jd);
                if (string.IsNullOrEmpty(fuid))
                    continue;
                string ainfo = XinManager.instance.AccountGet(fuid);
                if (!IsRegisted(ainfo))
                    continue;
                SimpleJSON.JSONNode ajn = SimpleJSON.JSON.Parse(ainfo);
                SimpleJSON.JSONClass ajc = JsonGen.UserInfo(
                   fuid,
                   ajn["entities"][0]["username"],
                   "userHandicon",
                   UIDToHuanXinID(fuid),
                   null,
                   SportMatchManager.instance.GetUserState(fuid),
                   SportMatchManager.instance.GetUserGroupID(fuid));
                friendArray.Add(ajc);
            }

            SimpleJSON.JSONClass result = new SimpleJSON.JSONClass();
            result.Add("status", JsonGen.Status(100));
            result.Add("friends", friendArray);
            Send(result);
        }

        string HuanXinIDToUID(string huanxinid)
        {
            if (huanxinid.StartsWith("huanxin_"))
                huanxinid = huanxinid.Substring("huanxin_".Length);
            return huanxinid;
        }

        string UIDToHuanXinID(string uid)
        {
            return "huanxin_" + uid;
        }

        [WebMethod(EnableSession = true)]
        public void startMatch(int type, int day, int timeStart, int timeEnd, float latitude, float longitude, int level, string invite)
        {
            string uuid = Context.Session["uuid"].ToString();

            LOG.Out("StartMatch: " + uuid);

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

            LOG.Out("GetMatch: " + uuid);

            string groupId = SportMatchManager.instance.GetUserGroupID(uuid);
            Send(JsonGen.MatchResult(groupId));
        }

        [WebMethod(EnableSession = true)]
        public void endMatch()
        {
            string uuid = Context.Session["uuid"].ToString();

            LOG.Out("EndMatch: " + uuid);

            SportMatchManager.instance.StopMatch(uuid);
            Send(JsonGen.Status(100));
        }

        [WebMethod(EnableSession = true)]
        public void endSport()
        {
            string uuid = Context.Session["uuid"].ToString();

            LOG.Out("EndSport: " + uuid);

            if (SportMatchManager.instance.IsInGroup(uuid))
            {
                XinManager.instance.GroupExit(SportMatchManager.instance.GetUserGroupID(uuid), uuid);
                SportMatchManager.instance.RemoveFormGroup(uuid);
            }
            Send(JsonGen.Status(100));
        }

        [WebMethod(EnableSession = true)]
        public void accept(string inviteUID)
        {
            string uuid = Context.Session["uuid"].ToString();

            LOG.Out("Accept: " + uuid);

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
            LOG.Out("Response : " + data);

            Context.Response.ContentType = "application/json";
            Context.Response.Charset = "utf-8"; //设置字符集类型
            Context.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Context.Response.Write(data);
            Context.Response.End();
        }
    }
}
