
namespace HXComm
{
    using System;
    using System.Text;
    using System.IO;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// 环信服务器端会员访问接口Demo
    /// Author：Mr.Hu
    /// QQ:346163801
    /// Email:346163801@qq.com
    /// 如有任何问题，可QQ或邮箱联系
    /// </summary>
    public class EaseXin
    {
        string reqUrlFormat = "https://a1.easemob.com/{0}/{1}/";
        public string clientID { get; set; }
        public string clientSecret { get; set; }
        public string appName { get; set; }
        public string orgName { get; set; }
        public string token { get; set; }
        public string easeMobUrl { get { return string.Format(reqUrlFormat, orgName, appName); } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="easeAppClientID">client_id</param>
        /// <param name="easeAppClientSecret">client_secret</param>
        /// <param name="easeAppName">应用标识之应用名称</param>
        /// <param name="easeAppOrgName">应用标识之登录账号</param>
        public EaseXin(string easeAppClientID, string easeAppClientSecret, string easeAppName, string easeAppOrgName)
        {
            this.clientID = easeAppClientID;
            this.clientSecret = easeAppClientSecret;
            this.appName = easeAppName;
            this.orgName = easeAppOrgName;
            this.token = QueryToken();
        }

        /// <summary>
        /// 使用app的client_id 和 client_secret登陆并获取授权token
        /// </summary>
        /// <returns></returns>
        string QueryToken()
        {
            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret)) { return string.Empty; }
            string cacheKey = clientID + clientSecret;
            if (System.Web.HttpRuntime.Cache.Get(cacheKey) != null &&
                System.Web.HttpRuntime.Cache.Get(cacheKey).ToString().Length > 0)
            {
                return System.Web.HttpRuntime.Cache.Get(cacheKey).ToString();
            }

            string postUrl = easeMobUrl + "token";
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            _build.AppendFormat("\"grant_type\": \"client_credentials\",\"client_id\": \"{0}\",\"client_secret\": \"{1}\"", clientID, clientSecret);
            _build.Append("}");

            string postResultStr = ReqUrl(postUrl, "POST", _build.ToString(), string.Empty);
            string token = string.Empty;
            int expireSeconds = 0;
            try
            {
                JObject jo = JObject.Parse(postResultStr);
                token = jo.GetValue("access_token").ToString();
                int.TryParse(jo.GetValue("expires_in").ToString(), out expireSeconds);
                //设置缓存
                if (!string.IsNullOrEmpty(token) && token.Length > 0 && expireSeconds > 0)
                {
                    System.Web.HttpRuntime.Cache.Insert(cacheKey, token, null, DateTime.Now.AddSeconds(expireSeconds), System.TimeSpan.Zero);
                }
            }
            catch { return postResultStr; }
            return token;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="password">密码</param>
        /// <returns>创建成功的用户JSON</returns>
        public string AccountCreate(string userName, string password)
        {
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            _build.AppendFormat("\"username\": \"{0}\",\"password\": \"{1}\"", userName, password);
            _build.Append("}");

            return AccountCreate(_build.ToString());
        }

        /// <summary>
        /// 创建用户(可以批量创建)
        /// </summary>
        /// <param name="postData">创建账号JSON数组--可以一个，也可以多个</param>
        /// <returns>创建成功的用户JSON</returns>
        public string AccountCreate(string postData) { return ReqUrl(easeMobUrl + "users", "POST", postData, token); }

        /// <summary>
        /// 获取指定用户详情
        /// </summary>
        /// <param name="userName">账号</param>
        /// <returns>会员JSON</returns>
        public string AccountGet(string userName) { return ReqUrl(easeMobUrl + "users/" + userName, "GET", string.Empty, token); }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>重置结果JSON(如：{ "action" : "set user password",  "timestamp" : 1404802674401,  "duration" : 90})</returns>
        public string AccountResetPwd(string userName, string newPassword) { return ReqUrl(easeMobUrl + "users/" + userName + "/password", "PUT", "{\"newpassword\" : \"" + newPassword + "\"}", token); }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userName">账号</param>
        /// <returns>成功返回会员JSON详细信息，失败直接返回：系统错误信息</returns>
        public string AccountDel(string userName) { return ReqUrl(easeMobUrl + "users/" + userName, "DELETE", string.Empty, token); }

        public string ReqUrl(string reqUrl, string method, string paramData, string token)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(reqUrl) as HttpWebRequest;
                request.Method = method.ToUpperInvariant();

                if (!string.IsNullOrEmpty(token) && token.Length > 1) { request.Headers.Add("Authorization", "Bearer " + token); }
                if (request.Method.ToString() != "GET" && !string.IsNullOrEmpty(paramData) && paramData.Length > 0)
                {
                    request.ContentType = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(paramData);
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        return result;
                    }
                }
            }
            catch (Exception ex) { return ex.ToString(); }
        }

        /*
         * {
            "groupname":"testrestgrp12", //群组名称，此属性为必须的
            "desc":"server create group", //群组描述，此属性为必须的
            "public":true, //是否是公开群，此属性为必须的
            "maxusers":300, //群组成员最大数（包括群主），值为数值类型，默认值200，此属性为可选的
            "approval":true, //加入公开群是否需要批准，默认值是false（加入公开群不需要群主批准），此属性为必选的，私有群必须为true
            "owner":"jma1", //群组的管理员，此属性为必须的
            "members":["jma2","jma3"] //群组成员，此属性为可选的，但是如果加了此项，数组元素至少一个（注：群主jma1不需要写入到members里面）
           }
         */

        public string GroupCreate()
        {
            string postData = "{\"groupname\":\"群名字\",\"desc\":\"描述\",\"public\":true,\"approval\":false,\"owner\":\"10000\",\"maxusers\":300}";
            return ReqUrl(easeMobUrl + "chatgroups", "POST", postData, token);
        }
    }
}
