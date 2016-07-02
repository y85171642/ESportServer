using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleJSON;

namespace Esports.space
{
    public class JsonGen
    {
        public static JSONClass Status(int code)
        {
            Dictionary<int, string> codeMsgDict = new Dictionary<int, string>()
            {
                { 100,"操作成功" },
                { 101,"参数错误" },
                { 102,"登录不成功，账号密码错误！" },
                { 103,"未知客户端！" },
                { 104,"原始密码不正确！" },
                { 105,"您已更新到最新版本！" },
                { 400,"其他错误" },
            };
            JSONClass jc = new JSONClass();
            jc.Add("code", JD(code));
            jc.Add("message", JD(codeMsgDict[code]));
            return jc;
        }

        public static JSONClass Login(JSONClass userInfo, int statusCode = 100)
        {
            JSONClass jc = new JSONClass();
            jc.Add("status", Status(statusCode));
            jc.Add("userInfo", userInfo);
            return jc;
        }

        public static JSONClass UserInfo(
            string userID = "",
            string nickName = "",
            string userHandicon = "",
            string huanxinid = "",
            string huanxinpassword = "",
            int state = -1,
            string stateid = "-1")
        {
            JSONClass jc = new JSONClass();
            jc.Add("userID", JD(userID));
            jc.Add("nickName", JD(nickName));
            jc.Add("userHandicon", JD(userHandicon));
            jc.Add("huanxinid", JD(huanxinid));
            jc.Add("huanxinpassword", JD(huanxinpassword));
            jc.Add("state", JD(state));
            jc.Add("stateid", JD(stateid));
            return jc;
        }

        public static JSONClass MatchResult(string groupID, int statusCode = 100)
        {
            JSONClass jc = new JSONClass();
            jc.Add("status", Status(statusCode));
            jc.Add("id", JD(groupID));
            jc.Add("success", JD(groupID != "-1" ? "1" : "0"));
            return jc;
        }

        public static JSONData JD(object obj)
        {
            if (obj is bool)
                return new JSONData((bool)obj);
            if (obj is int)
                return new JSONData((int)obj);
            if (obj is float)
                return new JSONData((float)obj);
            if (obj is double)
                return new JSONData((double)obj);
            if (obj is string)
                return new JSONData((string)obj);
            return new JSONData(obj.ToString());
        }
    }
}